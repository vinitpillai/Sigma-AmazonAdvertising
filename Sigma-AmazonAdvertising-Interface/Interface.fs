namespace Sigma.APIs.Interfaces

open System
open System.Linq
open System.Collections.Generic
open Newtonsoft.Json
open Sigma.Types.API
open Sigma.Lib
open Sigma.APIs
open Sigma.Types.API
open Sigma.Protocol.Client
open RabbitMQ.Client
open RabbitMQ.Client.Events
open RiakClient
open RiakClient.Models
open AmazonAdvertising_Types
open Sigma.APIs.Connectors


type AmazonAdvertising(rabbitConFact : ConnectionFactory, riak : IRiakClient, loggingExchange : string) = 
    inherit APIMicroserviceInterface("Twitter", rabbitConFact, riak, loggingExchange, 60 * 60)
    
    (** General request submission **)
    member private this.DoSubmit<'Return>(request : AmazonAdvertisingRequest, ?maxRetrys : int, 
                                          ?retryStrategy : int -> int) = 
        (** Defaults **)
        let maxRetry = defaultArg maxRetrys 0
        let strategy = defaultArg retryStrategy fib
        (** Submit **)
        this.clientProtocol.pollCompleteIntervalSecs <- 2
        this.clientProtocol.pollFailureThreshold <- 10
        this.Submit<'Return, AmazonAdvertisingError>(request, maxRetry, strategy)
    
    (** Get Application Access token **)
    member this.GetAppToken (cache : TimeSpan option) (keys : OAuthKeys) = 
        keys
        |> GetAppToken
        |> fun subRequest -> 
            { AmazonAdvertisingRequest.jobGuid = Guid.NewGuid()
              amazonAdvertisingUserName = ""
              request = subRequest
              cache = cache }
        |> fun request -> this.DoSubmit<GetAppTokenResponse> request

    (** Generate Amazon Report**)
    member this.AmazonCreateReport (cache : TimeSpan option) (reportRequet : AmazonReportRequestWithType) = 
        reportRequet
        |> AmazonReportRequest
        |> fun subRequest -> 
            { AmazonAdvertisingRequest.jobGuid = Guid.NewGuid()
              amazonAdvertisingUserName = ""
              request = subRequest
              cache = cache }
        |> fun request -> this.DoSubmit<AmazonReportResponse> request

    (** Get Amazon Report by Id **)
    member this.AmazonReportById (cache : TimeSpan option) (report : ReportId) = 
        report
        |> AmazonReportById
        |> fun subRequest -> 
            { AmazonAdvertisingRequest.jobGuid = Guid.NewGuid()
              amazonAdvertisingUserName = ""
              request = subRequest
              cache = cache }
        |> fun request -> this.DoSubmit<AmazonReport> request

  
