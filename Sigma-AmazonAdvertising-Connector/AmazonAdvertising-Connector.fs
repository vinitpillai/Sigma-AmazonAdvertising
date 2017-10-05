namespace Sigma.APIs.Connectors

open System
open System.Net
open System.Collections.Generic
open Sigma.Lib.Web
open Newtonsoft.Json
open Sigma.Types.API.AmazonAdvertising_Types
open System.IO
open System.Text

type AmazonAdvertising() = 
    (** Primary API URLs template **)
    //static member private urlTemplate = "https://https://advertising-api.amazon.com/{0}"
    static member private urlPostTemplate = "https://advertising-api-test.amazon.com/"
    static member private urlGetTemplate = "https://advertising-api-test.amazon.com/reports/"
    
    //static member private urlTemplate = "https://advertising-api-eu.amazon.com/{0}"
    (** Standard deserialisation mechanics **)
    static member PackageResponse<'T>(response : WebRequestResponse) = 
        try 
            response |> function 
            | WebRequestResponse.Response result -> 
                match result.Contains("error_description") with
                | true -> 
                    JsonConvert.DeserializeObject<AmazonAdvertisingAPIError>(result)
                    |> AmazonAdvertisingError.AmazonAdvertisingAPIError
                    |> AmazonAPIResponseWrapper.AmazonAdvertisingError
                | false -> JsonConvert.DeserializeObject<'T>(result) |> AmazonAPIResponseWrapper.Results
            | WebRequestResponse.Error error -> 
                error
                |> AmazonAdvertisingError.WebRequestError
                |> AmazonAPIResponseWrapper.AmazonAdvertisingError
        with ex -> 
            ex.Message
            |> AmazonAdvertisingError.GeneralError
            |> AmazonAPIResponseWrapper.AmazonAdvertisingError
    
    (** Get Amazon App access token **)
    static member GetAWSAppAccessToken(keys : OAuthKeys) = 
        (** Prepare request **)
        let oAuthUrl = "https://api.amazon.com/auth/o2/token"
        let authHeaderContent = 
            String.Format
                ("Basic {0}", 
                 Convert.ToBase64String
                     (Encoding.UTF8.GetBytes
                          (Uri.EscapeDataString(keys.consumerKey) + ":" + Uri.EscapeDataString((keys.consumerSecret)))))
        
        let headers = 
            [ "Authorization", authHeaderContent
              "Accept-Encoding", "gzip" ]
        
        let contentType = "application/x-www-form-urlencoded;charset=UTF-8"
        let decompressonMethods = DecompressionMethods.GZip ||| DecompressionMethods.Deflate
        let postBody = "grant_type=client_credentials"
        (** Submit response **)
        WebRequest.Transact(oAuthUrl, headers, contentType, decompressonMethods, "POST", postBody) 
        |> fun response -> AmazonAdvertising.PackageResponse<GetAppTokenResponse> response

    (**Post report query to Amazon**)
    static member CreateReport (apptoken: string) (recordType: string) (reportQuery : AmazonReportRequest) = 
        [ "Authorization" , String.Format("{0} {1}", "bearer", apptoken) ]
        |> fun headers ->  WebRequest.Transact(url = String.Format("{0}{1}", AmazonAdvertising.urlPostTemplate , recordType), headers = headers, requestMethod = "POST", postBody = (reportQuery |> JsonConvert.SerializeObject))
        |> fun response  -> AmazonAdvertising.PackageResponse<AmazonReportResponse> response

    (**Get query to Amazon**)
    static member GetReportById (apptoken: string) (reportQuery : ReportId) =
        [ "Authorization" , String.Format("{0} {1}", "bearer", apptoken) ] 
        |> fun headers -> WebRequest.Transact(AmazonAdvertising.urlGetTemplate + reportQuery.ToString(), headers)
        |> fun response  -> AmazonAdvertising.PackageResponse<AmazonReport> response
    
