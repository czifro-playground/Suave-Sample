namespace SuaveRestApi.Rest

  open Newtonsoft.Json
  open Newtonsoft.Json.Serialization
  open Suave
  open Suave.Filters
  open Suave.Successful
  open Suave.Operators
  open Suave.RequestErrors

  [<AutoOpen>]
  module RestFul =
    type RestResource<'a> = 
      {
        GetAll : unit -> 'a seq;
        Create : 'a -> 'a;
        Update : 'a -> 'a option;
        Delete : int -> unit;
      }

    let private jsonSerializerSettings =
      let settings = new JsonSerializerSettings()
      settings.ContractResolver <- 
        new CamelCasePropertyNamesContractResolver()
      settings

    let JSON v =
      JsonConvert.SerializeObject(v,jsonSerializerSettings)
      |> OK
      >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
      JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
      let getString rawForm =
        System.Text.Encoding.UTF8.GetString(rawForm)
      req.rawForm |> getString |> fromJson<'a>

    let rest resourceName resource =
      let resourcePath = "/" + resourceName
      let getAll = resource.GetAll() |> JSON

      let badRequest = BAD_REQUEST "Resource not found"
      let handleResource requestError = 
        function
          | Some r -> r |> JSON
          | _ -> requestError

      let resourceIdPath =
        let path = resourcePath + "/%d"
        new PrintfFormat<(int -> string),unit,string,string,int>(path)

      let deleteResourceById id =
        resource.Delete id
        NO_CONTENT

      choose [
        path resourcePath >=> choose [
          GET >=> getAll;
          POST >=> request (getResourceFromReq >> resource.Create >> JSON)
          PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
        ]
        DELETE >=> pathScan resourceIdPath deleteResourceById
      ]