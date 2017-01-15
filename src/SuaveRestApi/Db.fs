namespace SuaveRestApi.Db

  open System.Collections.Generic

  type Person =
    {
      Id : int;
      Name : string;
      Age : int;
      Email : string;
    }

  type Storage =
    {
      people : Dictionary<int, Person>;
      idCounter : int ref;
    }

  module Db =
    let private storage = 
      {
        people = new Dictionary<int, Person>();
        idCounter = ref 0
      }
    let getPeople() =
      storage.people.Values :> seq<Person>

    let createPerson person =
      incr storage.idCounter
      let id = !storage.idCounter
      let newPerson = { person with Id = id }
      storage.people.Add(id, newPerson)
      newPerson

    let updatePersonById personId personToBeUpdated =
      if storage.people.ContainsKey(personId) then
        let updatedPerson = { personToBeUpdated with Id = personId }
        storage.people.[personId] <- updatedPerson
        Some updatedPerson
      else
        None

    let updatePerson personToBeUpdated =
      updatePersonById personToBeUpdated.Id personToBeUpdated

    let deletePerson personId =
      storage.people.Remove(personId) |> ignore