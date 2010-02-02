namespace AdventureWorks

open System

type Product =
  {
    ID: Int32 option; 
    Name: String option;
    Number: String option;
    Color: String option;
    StandardCost: Single option;
    ListPrice: Single option;
    Size: String option;
    Weight: Single option;
    CategoryID: Int32 option;
    ModelID: Int32 option;
    SellStartDate: DateTime option;
    SellEndDate: DateTime option;
    DiscontinuedDate: DateTime option;
    ThumbNailPhoto: Byte[] option;
    ThumbnailPhotoFileName: String option;
    RowGuid: Guid option;
    ModifiedDate: DateTime option;
  }