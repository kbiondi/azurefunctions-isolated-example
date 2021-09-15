namespace WeatherStation.Function

open System
open System.Net

module SingleStation = 

    let getStringOption input = 
        try
            let v = input.ToString()
            Some v
        with 
        | _ ->
        let v = None 
        v

    let getDecimalOption input = 
        try
            let v = Decimal.Parse(input.ToString())
            Some v
        with 
        | _ ->
        let v = None 
        v

    let getStationObservationAsync station = 
        
        async {
            try
                let! observationResponse = Observation.AsyncLoad("https://api.weather.gov/stations/" + station.StationId + "/observations/latest?require_qc=true")    
                let x = observationResponse.Properties    
                
                let entry = ({                        
                        Id = getStringOption(x.Id);                            
                        Temperature = getDecimalOption(x.Temperature); 
                        Dewpoint = getDecimalOption(x.Dewpoint);
                        WindDirection = getDecimalOption(x.WindDirection); 
                        WindSpeed = getDecimalOption(x.WindSpeed); 
                        BarometricPressure = getDecimalOption(x.BarometricPressure); 
                        Visibility = getDecimalOption(x.Visibility); 
                        RelativeHumidity = getDecimalOption(x.RelativeHumidity)
                        })                        

                return entry

            with
                | :? WebException as e -> 
                    let entry = ({ 
                        Id = None; 
                        Temperature = None; 
                        Dewpoint = None; 
                        WindDirection = None; 
                        WindSpeed = None; 
                        BarometricPressure = None;
                        Visibility = None; 
                        RelativeHumidity = None})

                    return entry   
        }
    
    
