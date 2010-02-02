namespace AdventureWorks

open AdventureWorks
open MsSqlReader

type ProductServiceMsSql(cn) =
  let readAll() =
    seq {
      use cmd = createCommand cn "select ProductID, Name, ProductNumber, Color, StandardCost, ListPrice, Size, Weight, ProductCategoryID, ProductModelID, SellStartDate, SellEndDate, DiscontinuedDate, ThumbNailPhoto, ThumbnailPhotoFileName, rowguid, ModifiedDate from SalesLT.Product"
      use r = cmd.ExecuteReader()
      while r.Read() do
        yield
          {
            ID = readInt32 r 0;
            Name = readString r 1;
            Number = readString r 2;
            Color = readNullableString r 3;
            StandardCost = readNullableDecimalAsSingle r 4;
            ListPrice = readNullableDecimalAsSingle r 5;
            Size = readNullableString r 6;
            Weight = readNullableDecimalAsSingle r 7;
            CategoryID = readNullableInt32 r 8;
            ModelID = readNullableInt32 r 9;
            SellStartDate = readDateTime r 10;
            SellEndDate = readNullableDateTime r 11;
            DiscontinuedDate = readNullableDateTime r 12;
            ThumbNailPhoto = readNullableBytes r 13;
            ThumbnailPhotoFileName = readNullableString r 14;
            RowGuid = readGuid r 15;
            ModifiedDate = readDateTime r 16;
          }
    }
  member x.ReadAll = readAll