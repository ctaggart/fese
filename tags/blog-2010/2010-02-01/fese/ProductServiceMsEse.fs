namespace AdventureWorks

open System
open System.IO
open MsEse

module private Names =
  // Table
  let Product = "Product"
  // Columns
  let ID = "ID"
  let Name = "Name"
  let Number = "Number"
  let Color = "Color"
  let StandardCost = "StandardCost"
  let ListPrice = "ListPrice"
  let Size = "Size"
  let Weight = "Weight"
  let CategoryID = "CategoryID"
  let ModelID = "ModelID"
  let SellStartDate = "SellStartDate"
  let SellEndDate = "SellEndDate"
  let DiscontinuedDate = "DiscontinuedDate"
  let ThumbNailPhoto = "ThumbNailPhoto"
  let ThumbnailPhotoFileName = "ThumbnailPhotoFileName"
  let RowGuid = "RowGuid"
  let ModifiedDate = "ModifiedDate"

type AdventureWorksEse(file) =
  let fileExists = File.Exists file
  
  let ese = getInstance file
  do 
    ese.Parameters.Recovery <- false // useful to turn off during development
    ese.Init()

  // create table and columns for a new file
  do
    if fileExists <> true then
      use sn = openSession ese
      let db = createDatabase sn file
      let t = createTable sn db Names.Product
      addColumnInt32NotNull sn t Names.ID
      addColumnTextUnicodeNotNull sn t Names.Name 100 // bytes, 50 Unicode characters
      addColumnTextUnicode sn t Names.Number 50 // bytes, 25 Unicode characters
      addColumnTextUnicode sn t Names.Color 30 // bytes, 15 Unicode characters
      addColumnSingleNotNull sn t Names.StandardCost
      addColumnSingleNotNull sn t Names.ListPrice
      addColumnTextUnicode sn t Names.Size 10 // bytes, 5 Unicode characters
      addColumnSingle sn t Names.Weight
      addColumnInt32 sn t Names.CategoryID
      addColumnInt32 sn t Names.ModelID
      addColumnDateTimeNotNull sn t Names.SellStartDate
      addColumnDateTime sn t Names.SellEndDate
      addColumnDateTime sn t Names.DiscontinuedDate
      addColumnBytes sn t Names.ThumbNailPhoto 8000 // SQL Server varbinary max
      addColumnTextUnicode sn t Names.ThumbnailPhotoFileName 100 // bytes, 50 Unicode characters
      addColumnGuidNotNull sn t Names.RowGuid
      addColumnDateTimeNotNull sn t Names.ModifiedDate
 
      createIndexPrimary sn t Names.ID
      createIndexUnique sn t Names.Name
      createIndexUnique sn t Names.Number
      createIndexUnique sn t Names.RowGuid
  
  member x.File with get() = file
  member x.Ese with get() = ese

  interface IDisposable with
    member x.Dispose() =
      ese.Dispose()

// wraps a ESE session, so create a new instance for each thread
// http://msdn.microsoft.com/en-us/library/aa964734(EXCHG.10).aspx
type ProductServiceMsEse(ese:AdventureWorksEse) =
  let sn = openSession ese.Ese
  
  let table =
    let db =
      attachDatabase sn ese.File
      openDatabase sn ese.File
    getTable sn db Names.Product

  // get columns
  let getColumn name =
    MsEse.getColumn sn table name
  let columnID = getColumn Names.ID
  let columnName = getColumn Names.Name
  let columnNumber = getColumn Names.Number
  let columnColor = getColumn Names.Color
  let columnStandardCost = getColumn Names.StandardCost
  let columnListPrice = getColumn Names.ListPrice
  let columnSize = getColumn Names.Size
  let columnWeight = getColumn Names.Weight
  let columnCategoryID = getColumn Names.CategoryID
  let columnModelID = getColumn Names.ModelID
  let columnSellStartDate = getColumn Names.SellStartDate
  let columnSellEndDate = getColumn Names.SellEndDate
  let columnDiscontinuedDate = getColumn Names.DiscontinuedDate
  let columnThumbNailPhoto = getColumn Names.ThumbNailPhoto
  let columnThumbnailPhotoFileName = getColumn Names.ThumbnailPhotoFileName
  let columnRowGuid = getColumn Names.RowGuid
  let columnModifiedDate = getColumn Names.ModifiedDate

  let insert sn (p:Product) =
    let t = table
    let setColumns() =
      writeInt32 sn t columnID p.ID
      writeString sn t columnName p.Name
      writeString sn t columnNumber p.Number
      writeString sn t columnColor p.Color
      writeSingle sn t columnStandardCost p.StandardCost
      writeSingle sn t columnListPrice p.ListPrice
      writeString sn t columnSize p.Size
      writeSingle sn t columnWeight p.Weight
      writeInt32 sn t columnCategoryID p.CategoryID
      writeInt32 sn t columnModelID p.ModelID
      writeDateTime sn t columnSellStartDate p.SellStartDate
      writeDateTime sn t columnDiscontinuedDate p.DiscontinuedDate
      writeBytes sn t columnThumbNailPhoto p.ThumbNailPhoto
      writeString sn t columnThumbnailPhotoFileName p.ThumbnailPhotoFileName
      writeGuid sn t columnRowGuid p.RowGuid
      writeDateTime sn t columnModifiedDate p.ModifiedDate
    MsEse.insert sn t setColumns

  let read() =
    let t = table
    {
      ID = readInt32 sn t columnID;
      Name = readString sn t columnName;
      Number = readString sn t columnNumber;
      Color = readString sn t columnColor;
      StandardCost = readSingle sn t columnStandardCost;
      ListPrice = readSingle sn t columnListPrice;
      Size = readString sn t columnSize;
      Weight = readSingle sn t columnWeight;
      CategoryID = readInt32 sn t columnCategoryID;
      ModelID = readInt32 sn t columnModelID;
      SellStartDate = readDateTime sn t columnSellStartDate;
      SellEndDate = readDateTime sn t columnSellEndDate;
      DiscontinuedDate = readDateTime sn t columnDiscontinuedDate;
      ThumbNailPhoto = readBytes sn t columnThumbNailPhoto;
      ThumbnailPhotoFileName = readString sn t columnThumbNailPhoto;
      RowGuid = readGuid sn t columnRowGuid;
      ModifiedDate = readDateTime sn t columnModifiedDate;
    }
    
  member x.Insert product =
    let fn() = insert sn product
    transact sn fn

  member x.InsertAll products =
    let fn() = products |> Seq.iter (insert sn)
    transact sn fn

  member x.ReadAll() =
    setCurrentIndexToPrimary sn table
    readAll sn table read

  member x.ReadAllByName() =
    setCurrentIndex sn table Names.Name
    readAll sn table read

  member x.ReadAllByNumber() =
    setCurrentIndex sn table Names.Number
    readAll sn table read

  interface IDisposable with
    member x.Dispose() =
      table.Dispose()
      sn.Dispose()