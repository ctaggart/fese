module AdventureWorks.Program

open System

/// Establishes a connection to SQL Server. Creates the ESE database file. Fills the ESE database.
let transferProducts() =
  use ese = new AdventureWorksEse("AdventureWorks.db")
  use svcEse = new ProductServiceMsEse(ese)
  use cn = MsSqlReader.openConnection @"server=.\sqlexpress; Integrated Security=true; Initial Catalog=AdventureWorksLT2008R2"
  let svcSql = ProductServiceMsSql cn
  svcSql.ReadAll() |> svcEse.InsertAll

/// print the first n products indexed by name
let printProducts n =
  let print (p:Product) =
    printfn "%s %s" p.Number.Value p.Name.Value
  use ese = new AdventureWorksEse("AdventureWorks.db")
  use svc = new ProductServiceMsEse(ese)
  svc.ReadAllByName()
  |> Seq.take n
  |> Seq.iter print

let main() =
  transferProducts()
  printProducts 10
  Console.ReadKey false |> ignore

main()