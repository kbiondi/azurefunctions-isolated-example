namespace WeatherStation.Function

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
// Takes an HTTP request that contains State and County in the paramenters
// and goes out to the government weather station api https://api.weather.gov/ and give me
// weather station observation in that county and save it to an Azure blob storerage
//https://www.weather.gov/documentation/services-web-api
//https://weather-gov.github.io/api/
//blobContainerName = datalake
//blobFolderPath = landing/paa/fsharp_pipeline_test/observations/ --from storage container
//connectionString = connection string for azu004stapd004
//azu004stapd004 => Access Keys, save as environment variable on local PC


module HttpTrigger =
    
    [<FunctionName("WeatherStationApi")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            
            log.LogInformation("Request received...")

            let tryGetState =
              try
                match req.Query.["State"].[0] <> "" with
                | true -> Some req.Query.["State"].[0]
                | false -> None
              with _ -> None

            let tryGetCounty = 
              try
                match req.Query.["County"].[0] <> "" with
                | true -> Some req.Query.["County"].[0]
                | false -> None
              with _ -> None

            let isState = 
              match tryGetState with
              | Some s -> true
              | None -> false

            let isCounty = 
              match tryGetCounty with
              | Some c -> true
              | None -> false

            let badRequestResponse = "Your request is missing required parameters."              

            let response = 
              match isState && isCounty with
              | true -> OkObjectResult(WeatherStationData.getData log tryGetState.Value tryGetCounty.Value) :> IActionResult
              | false -> BadRequestObjectResult(badRequestResponse) :> IActionResult
                  
            return response

        } |> Async.StartAsTask