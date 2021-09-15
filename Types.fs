namespace WeatherStation.Function

open FSharp.Data

[<AutoOpen>]
module Types = 
    //Define Types

    type CountyList = JsonProvider<"https://api.weather.gov/zones?area=CA&limit=100">
    type StationList = JsonProvider<"https://api.weather.gov/stations?state=CA&limit=10", InferTypesFromValues=false>
    type Observation = JsonProvider<"https://api.weather.gov/stations/DOGSV/observations/latest?require_qc=true">

    type County =
        {   CountyId: string
            CountyName: string
        }

    type Station = 
        { StationId: string
          StationName: string
          Elevation: decimal
          Latitude: decimal
          Longitude: decimal
        }

    type Latest = 
        { Id: string option
          Temperature: decimal option
          Dewpoint: decimal option
          WindDirection: decimal option
          WindSpeed: decimal option
          BarometricPressure: decimal option
          Visibility: decimal option
          RelativeHumidity: decimal option   
        }
