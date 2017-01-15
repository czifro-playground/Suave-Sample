namespace SuaveRestApi

  open Suave.Web
  open Suave.Successful
  open SuaveRestApi.Rest
  open SuaveRestApi.Db

  module App =
  
    [<EntryPoint>]
    let main argv =
      let restResource = 
        { 
          GetAll = Db.getPeople; 
          Create = Db.createPerson; 
          Update = Db.updatePerson;
          Delete = Db.deletePerson;
        }
      let personWebPart = rest "people" restResource
      startWebServer defaultConfig personWebPart
      0
