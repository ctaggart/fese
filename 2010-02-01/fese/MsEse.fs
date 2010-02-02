/// F# functions for Microsoft's Extensable Storage Engine
/// wraps ESENT Managed Interop, Esent.Interop.dll
/// http://managedesent.codeplex.com/
/// that wraps %systemroot%\system32\esent.dll
/// http://msdn.microsoft.com/en-us/library/ms683070(EXCHG.10).aspx
module MsEse

open System
open System.IO
open System.Text
open Microsoft.Isam.Esent.Interop
open Microsoft.Isam.Esent.Interop.Vista

type ColumnID = JET_COLUMNID

let private fileFullPath name =
  (FileInfo name).FullName

let getInstance file =
  let ese = new Instance(fileFullPath file)
  ese.Parameters.CircularLog <- true
  ese.Parameters.AlternateDatabaseRecoveryDirectory <- (Directory.GetParent file).FullName
  ese

let openSession (ese:Instance) =
  new Session(ese.JetInstance)

// Column Types

/// creates a definition of a column to store an int32
let columnInt32() =
  let c = JET_COLUMNDEF()
  c.coltyp <- JET_coltyp.Long
  c
  
let columnInt32NotNull() =
  let c = columnInt32()
  c.grbit <- ColumndefGrbit.ColumnNotNULL
  c
  
let columnTextAsciiNoMax() =
  let c = JET_COLUMNDEF()
  c.coltyp <- JET_coltyp.Text
  c.cp <- JET_CP.ASCII
  c
  
let columnTextAscii max =
  let c = columnTextAsciiNoMax()
  c.cbMax <- max
  c

let columnTextUnicodeNoMax() =
  let c = JET_COLUMNDEF()
  c.coltyp <- JET_coltyp.Text
  c.cp <- JET_CP.Unicode
  c

let columnTextUnicode max =
  let c = columnTextUnicodeNoMax()
  c.cbMax <- max
  c
  
let columnTextUnicodeNotNull max =
  let c = columnTextUnicode max
  c.grbit <- ColumndefGrbit.ColumnNotNULL
  c

let columnSingle() =
  let c = JET_COLUMNDEF()
  c.coltyp <- JET_coltyp.IEEESingle
  c
  
let columnSingleNotNull() =
  let c = columnSingle()
  c.grbit <- ColumndefGrbit.ColumnNotNULL
  c

let columnDateTime() =
  let c = JET_COLUMNDEF()
  c.coltyp <- JET_coltyp.DateTime
  c
  
let columnDateTimeNotNull() =
  let c = columnDateTime()
  c.grbit <- ColumndefGrbit.ColumnNotNULL
  c

let columnGuid() =
  let c = JET_COLUMNDEF()
  c.coltyp <- VistaColtyp.GUID
  c
  
let columnGuidNotNull() =
  let c = columnGuid()
  c.grbit <- ColumndefGrbit.ColumnNotNULL
  c

let columnBytes max =
  let c = JET_COLUMNDEF()
  if max <= 255 then
    c.coltyp <- JET_coltyp.Binary
  else
    c.coltyp <- JET_coltyp.LongBinary
  c.cbMax <- max
  c

// Database Definition

let createDatabase (sn:Session) file =
  let mutable dbId = JET_DBID.Nil
  Api.JetCreateDatabase (sn.JetSesid, fileFullPath file, null, &dbId, CreateDatabaseGrbit.OverwriteExisting)
  dbId

let createTableWithPagesDensity (sn:Session) dbId name pages density =
  let mutable tableId = JET_TABLEID.Nil
  Api.JetCreateTable (sn.JetSesid, dbId, name, pages, density, &tableId)
  tableId

let createTable sn dbId name =
  createTableWithPagesDensity sn dbId name 8 100

let addColumn (sn:Session) tableId name def =
  let mutable columnId = JET_COLUMNID.Nil
  Api.JetAddColumn(sn.JetSesid, tableId, name, def, null, 0, &columnId)

let addColumnInt32 sn tableId name =
  addColumn sn tableId name (columnInt32())

let addColumnInt32NotNull sn tableId name =
  addColumn sn tableId name (columnInt32NotNull())

let addColumnTextUnicode sn tableId name max =
  addColumn sn tableId name (columnTextUnicode max)

let addColumnTextUnicodeNotNull sn tableId name max =
  addColumn sn tableId name (columnTextUnicodeNotNull max)

let addColumnSingle sn tableId name =
  addColumn sn tableId name (columnSingle())

let addColumnSingleNotNull sn tableId name =
  addColumn sn tableId name (columnSingleNotNull())

let addColumnDateTime sn tableId name =
  addColumn sn tableId name (columnDateTime())

let addColumnDateTimeNotNull sn tableId name =
  addColumn sn tableId name (columnDateTimeNotNull())

let addColumnGuid sn tableId name =
  addColumn sn tableId name (columnGuid())

let addColumnGuidNotNull sn tableId name =
  addColumn sn tableId name (columnGuidNotNull())

let addColumnBytes sn tableId name max =
  addColumn sn tableId name (columnBytes max)

let createIndex (sn:Session) tableId name options (columns:seq<string*bool>) density =
  let key = StringBuilder()
  let nullChar = '\u0000'
  for name,ascending in columns do
    if ascending then
      key.Append '+' |> ignore
    else
      key.Append '-' |> ignore
    key.Append name |> ignore
    key.Append nullChar |> ignore
  key.Append nullChar |> ignore  
  let keyStr = key.ToString()
  Api.JetCreateIndex(sn.JetSesid, tableId, name, options, keyStr, keyStr.Length, density)

let createIndexPrimary sn tableId column =
  let options = CreateIndexGrbit.IndexPrimary ||| VistaGrbits.IndexDisallowTruncation
  createIndex sn tableId column options [column,true] 100

let createIndexPrimaryDesc sn tableId column =
  createIndex sn tableId column CreateIndexGrbit.IndexPrimary [column,false] 100

let createIndexUnique sn tableId column =
  let options = CreateIndexGrbit.IndexUnique ||| CreateIndexGrbit.IndexIgnoreAnyNull ||| VistaGrbits.IndexDisallowTruncation
  createIndex sn tableId column options [column,true] 100

// Use Database

let attachDatabase (sn:Session) file =
  match Api.JetAttachDatabase (sn.JetSesid, file, AttachDatabaseGrbit.None) with
  | JET_wrn.Success -> ()
  | JET_wrn.DatabaseAttached -> ()
  | wrn -> failwith (sprintf "unable to attach to database file: %s, error: %A" file wrn)

let openDatabase (sn:Session) file =
  let mutable dbId = JET_DBID.Nil
  match Api.JetOpenDatabase (sn.JetSesid, file, null, &dbId, OpenDatabaseGrbit.None) with
  | JET_wrn.Success -> dbId
  | wrn -> failwith (sprintf "unable to open database file: %s, error: %A" file wrn)

let getTable (sn:Session) dbId name =
  new Table (sn.JetSesid, dbId, name, OpenTableGrbit.None)

let writeString (sn:Session) (t:Table) columnId (v:String option) =
   if v.IsSome then 
     Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value, Encoding.Unicode, SetColumnGrbit.None)

let writeInt32 (sn:Session) (t:Table) columnId (v:Int32 option) =
  if v.IsSome then Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value)

let writeSingle (sn:Session) (t:Table) columnId  (v:Single option) =
  if v.IsSome then Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value)

let writeDateTime (sn:Session) (t:Table) columnId (v:DateTime option) =
  if v.IsSome then Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value)

let writeBytes (sn:Session) (t:Table) columnId (v:Byte[] option) =
  if v.IsSome then Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value)

let writeGuid (sn:Session) (t:Table) columnId (v:Guid option) =
  if v.IsSome then Api.SetColumn(sn.JetSesid, t.JetTableid, columnId, v.Value)

let getColumn (sn:Session) (t:Table) name =
  Api.GetTableColumnid(sn.JetSesid, t.JetTableid, name)

let insert (sn:Session) (t:Table) fn =
  use update = new Update(sn.JetSesid, t.JetTableid, JET_prep.Insert)
  fn()
  update.Save()

let setCurrentIndex (sn:Session) (t:Table) index =
  Api.JetSetCurrentIndex(sn.JetSesid, t.JetTableid, index)

let setCurrentIndexToPrimary sn t =
  setCurrentIndex sn t null

let readInt32 (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumnAsInt32(sn.JetSesid, t.JetTableid, column)
  if v.HasValue then Some v.Value else None

let readString (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumnAsString(sn.JetSesid, t.JetTableid, column)
  if v = null then None else Some v

let readSingle (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumnAsFloat(sn.JetSesid, t.JetTableid, column)
  if v.HasValue then Some v.Value else None

let readDateTime (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumnAsDateTime(sn.JetSesid, t.JetTableid, column)
  if v.HasValue then Some v.Value else None

let readBytes (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumn(sn.JetSesid, t.JetTableid, column)
  if v.Length = 0 then None else Some v

let readGuid (sn:Session) (t:Table) column =
  let v = Api.RetrieveColumnAsGuid(sn.JetSesid, t.JetTableid, column)
  if v.HasValue then Some v.Value else None

let tryMoveFirst (sn:Session) (t:Table) =
  Api.TryMoveFirst(sn.JetSesid, t.JetTableid)

let tryMoveLast (sn:Session) (t:Table) =
  Api.TryMoveLast(sn.JetSesid, t.JetTableid)

let tryMoveNext (sn:Session) (t:Table) =
  Api.TryMoveNext(sn.JetSesid, t.JetTableid)

let tryMovePrevious (sn:Session) (t:Table) =
  Api.TryMovePrevious(sn.JetSesid, t.JetTableid)

/// execute the function in a transaction and LazyFlush commit
let transact (sn:Session) fn =
  use tn = new Transaction(sn.JetSesid)
  fn()
  tn.Commit CommitTransactionGrbit.LazyFlush

let readAll (sn:Session) (t:Table) read =
  seq {
    if tryMoveFirst sn t then
      yield read()
      while tryMoveNext sn t do
        yield read()
  }