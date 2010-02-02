module MsSqlReader

open System
open System.Data
open System.Data.Sql
open System.Data.SqlClient

let openConnection connectionString =
   let cn = new SqlConnection(connectionString)
   cn.Open()
   cn

let createCommand cn sql =
  new SqlCommand(sql, cn)

let readString (r:SqlDataReader) i =
  r.GetString i |> Some

let readNullableString (r:SqlDataReader) i =
  if r.IsDBNull i then None else readString r i

let readInt32 (r:SqlDataReader) i =
  r.GetInt32 i |> Some

let readNullableInt32 (r:SqlDataReader) i =
  if r.IsDBNull i then None else readInt32 r i

let readSingle (r:SqlDataReader) i =
  r.GetFloat i |> Some

let readNullableSingle (r:SqlDataReader) i =
  if r.IsDBNull i then None else readSingle r i

let readDateTime (r:SqlDataReader) i =
  r.GetDateTime i |> Some

let readNullableDateTime (r:SqlDataReader) i =
  if r.IsDBNull i then None else readDateTime r i

let readGuid (r:SqlDataReader) i =
  r.GetGuid i |> Some

let readNullableGuid (r:SqlDataReader) i =
  if r.IsDBNull i then None else readGuid r i

let readDecimal (r:SqlDataReader) i =
  r.GetDecimal i |> Some

let readNullableDecimal (r:SqlDataReader) i =
  if r.IsDBNull i then None else readDecimal r i

let readNullableDecimalAsSingle r i =
  match readNullableDecimal r i with
  | None -> None
  | Some v -> Decimal.ToSingle v |> Some

let readBytes (r:SqlDataReader) i =
  let length = r.GetBytes(i, 0L, null, 0, 0) |> int
  let mutable bytes = Array.zeroCreate<byte> length
  r.GetBytes(i, 0L, bytes, 0, length) |> ignore
  bytes |> Some

let readNullableBytes (r:SqlDataReader) i =
  if r.IsDBNull i then None else readBytes r i