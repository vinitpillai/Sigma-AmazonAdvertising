namespace Sigma.Types.API

[<AutoOpen>]
module AmazonAdvertising_Types = 
    open System
    open Sigma.Types.Job
    open Sigma.Lib.Web
    
    //***************************//
    // GENERAL & COMPONENT TYPES //
    //***************************//
    (** Represents amazon advertising access token stored in Riak **)
    type Credentials = 
        { AccessToken : string }

    (** Keys for a get tokens request **)
    type OAuthKeys = {
        consumerKey    : string // Client Id
        consumerSecret : string // Client Secret
    }
    
    (** Response to a GetAppToken request **)
    type GetAppTokenResponse = {
        token_type   : string
        access_token : string
    }

    (** Error type, as returned in JSON form from the Amazon API **)
    type AmazonAdvertisingAPIError = 
        { error : string
          error_description : string }
    
    (** ALL possible errors we'll handle when dealing with the Amazon API **)
    type AmazonAdvertisingError = 
        | AmazonAdvertisingAPIError of AmazonAdvertisingAPIError
        | WebRequestError of WebRequestError
        | GeneralError of string
        | NoAPIKey
        | StorageError
    
    (** Generic response wrapper **)
    type AmazonAPIResponseWrapper<'T> = 
        | Results of 'T
        | AmazonAdvertisingError of AmazonAdvertisingError

    type AmazonErrorObject = 
        { errorCode : string
          details : string}
        (**Amazon Report response in JSON**)
    type AmazonReportResponse = 
        { reportId : string
          recordType : string
          status : string
          statusDetails : string
          location : string
          fileSize : int }

    type ResponseObject = 
        | Error of AmazonErrorObject
        | Response of AmazonReportResponse
    
    type AmazonAPIResponse  = 
        { StatusCode : int
          ResponseObject : ResponseObject}
    
    (** Query string input **)
    type QueryStringInput = 
        { queries : string }
    
    (**OAuth 2.0 input**)
    type OAuthInput = 
        { client_id : string
          scope : string list
          response_type : string
          redirect_uri : string
          state : string }
    
    (** OAuth Authentication Header**)
    type AuthenticationHeader = 
        { InputQuery : QueryStringInput
          BearerToken : string
          Host : Uri
          AmazonAdvertisingAPIScope : string
          ContentType : string }
    
    (** Amazon Account info object**)
    type AmazonAccountInfo = 
        { marketplaceStringId : string
          sellerStringId : string
          brandEntityId : string
          brandName : string }
    
    (** Amazon Profile object**)
    type AmazonProfile = 
        { profileId : string
          countryCode : string
          currencyCode : string
          dailyBudget : decimal
          timezone : string
          accountInfo : AmazonAccountInfo }
    
    (**Amazon API General Response**)
    type AmazonResponse = 
        { code : string
          details : string }
    
    (**Amazon API Profile Response**)
    type AmazonProfileResponse = 
        { profileId : string
          amazonResponse : AmazonResponse }
    
    (** Amazon Profiles list**)
    type AmazonProfileList = 
        { profiles : AmazonProfile list }
    
    (** Amazon API profile id as list**)
    type ProfileId = 
        { profileId : string list }
    
    (**Register Profile for a Country**)
    type CountryCode = 
        { countryCode : string list }
    
    (**Register a brand**)
    type RegisterBrand = 
        { countryCode : CountryCode
          brand : string }
    
    (**Amazon Campaign ID**)
    type CampaignId = string
    
    (**List Campaign Request**)
    type AmazonCampaignListRequest = 
        { startIndex : int
          count : int
          campaignType : string
          stateFilter : string
          name : string
          campaignIdFilter : string }
    
    (**Amazon Campaign Response**)
    type AmazonCampaign = 
        { campaignId : int
          name : string
          campaignType : string
          targetingType : string
          state : string
          dailyBudget : string
          startDate : string
          endDate : string
          premiumBidAdjustment : bool }
    
    (**Amazon CampaignEx response**)
    type AmazonCampaignEx = 
        { campaignDetails : AmazonCampaign
          creationDate : string
          lastUpdatedDate : string
          servingStatus : string }
    
    (**Amazon API Campaign Response**)
    type AmazonCampaignResponse = 
        { campaign : string
          amazonResponse : AmazonResponse }
    
    //***************************//
    // Performance Reporting     //
    //***************************//
    (** Amazon reporting request **)
    type AmazonReportRequest = 
        { recordType : string
          campaignType : string
          segment : string
          reportDate : string
          metrics : string }
    
    (**Amazon Report ID**)
    type ReportId = string
    
    type AmazonReportRequestWithType = 
        { reportType : string
          reportRequest : AmazonReportRequest}
    
    (**Amazon Report object in JSON**) 
    type AmazonReportObject = 
        { keywork : string
          query : string
          impression : int
          clicks : int }
    
    (**Amazon Report in JSON**) type AmazonReport = 
        { report : AmazonReportObject list }
    
    //*************************//
    // BLENDED REQUEST TYPE    //
    //*************************//
    (** AmazonAdvertising sub-request definition, which is an option type of the individial requests we can make **)
    type AmazonAdvertisingSubRequest = 
        | GetAppToken of OAuthKeys
        | AmazonReportRequest of AmazonReportRequestWithType
        | AmazonReportById of ReportId
    
    (** AmazonAdvertising request **)
    type AmazonAdvertisingRequest = 
        { jobGuid : Guid
          amazonAdvertisingUserName : string
          request : AmazonAdvertisingSubRequest
          cache : TimeSpan option }
        interface IJobRequestInterface with
            member x.jobType = "AmazonAdvertising"
            member x.jobID = x.jobGuid.ToString()
            member x.cacheLimit = x.cache
