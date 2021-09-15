namespace WeatherStation.Function

open Azure.Storage.Blobs
open System.IO
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open System


module WeatherStationData =

    //Get API Data and Save to Data Lake
    let getData (log: ILogger) state county = 
        
        async {
            log.LogInformation("Data collection started...")
            try
                let timeStamp = DateTime.Now.ToString("s")
                let blobContainer = "datalake"
                let blobFolderPath = "landing/paa/fsharp_pipeline_test/observations/"
                let connection = Environment.GetEnvironmentVariable("ConnectionStrings__azu004stapd004", EnvironmentVariableTarget.Process)
                let! countyResponse = CountyList.AsyncLoad("https://api.weather.gov/zones?area=" + state)   
                
                log.LogInformation("Selecting county...")       
                let selectedCounty =                     
                    countyResponse.Features
                    |> Seq.filter(fun x -> x.Properties.Name = county)
                    |> Seq.map(fun x -> { CountyId = x.Properties.Id; CountyName = x.Properties.Name } )
                    |> Seq.exactlyOne

                log.LogInformation("County selected successfully.")
                log.LogInformation("Compiling station list...")

                let! stationResponse = StationList.AsyncLoad("https://api.weather.gov/stations?state=" + state)   

                let stationList = 
                    stationResponse.Features
                    |> Seq.filter(fun x -> x.Properties.County = selectedCounty.CountyId)
                    |> Seq.map(fun x -> {
                        StationId = x.Properties.StationIdentifier; 
                        StationName = x.Properties.Name; 
                        Elevation = x.Properties.Elevation.Value; 
                        Latitude = x.Geometry.Coordinates.[0]; 
                        Longitude = x.Geometry.Coordinates.[1]
                        })
                    |> Seq.toList

                log.LogInformation("Station list compiled successfully.")

                log.LogInformation("Collecting observations...")
                
                let observations = 
                    stationList
                    |>List.map SingleStation.getStationObservationAsync                    
                    |>Async.Parallel
                    |>Async.RunSynchronously

                let obsLength = 
                    observations
                    |> Seq.length
                
                log.LogInformation("Successfully collected " + obsLength.ToString() + " observations.")
                log.LogInformation("Serializing JSON...")

                let observationJson = JsonConvert.SerializeObject(observations)

                log.LogInformation("JSON serialized successfully.")
                log.LogInformation("Uploading to Data Lake...")

                let observationName = "obs_" + timeStamp

                //Upload Observation                
                let obsBlob = 
                    let blobServiceClient = BlobServiceClient(connection)
                    let container = blobServiceClient.GetBlobContainerClient(blobContainer)
                    container.GetBlobClient(blobFolderPath + observationName + ".json")
    
                let msObs = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(observationJson))

                obsBlob.UploadAsync(msObs) |> ignore              

                log.LogInformation(observationName + ".json uploaded to Data Lake Successfully.")

                return "Weather API Data Uploaded to Data Lake Successfully"

            with
            | _ -> return "An error occurred in the upload process. Please see the log file for details."

        } |> Async.StartAsTask