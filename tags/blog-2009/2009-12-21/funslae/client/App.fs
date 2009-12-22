namespace Funslae

open System
open System.Windows
open System.Windows.Browser
open System.Windows.Controls
open System.Windows.Media
open Microsoft.Maps.MapControl
open Microsoft.Maps.MapControl.Core

type ScriptableMap() =
  inherit Map()
  [<ScriptableMember>]
  member this.SetCenter(latitude, longitude) = 
    this.Center <- Location(latitude, longitude)
  [<ScriptableMember>]
  member this.SetZoomLevel(miles) =
    this.ZoomLevel <- miles

type App() as app =
  inherit Application()
  do
    app.Startup.Add(fun args ->
      let grid = Grid()
      app.RootVisual <- grid

      let map = ScriptableMap()
      grid.Children.Add map
      
      map.CredentialsProvider <- {
        new CredentialsProvider() with
          member x.GetCredentials(action) =
            action.Invoke(
              let credentials = Credentials()
              credentials.ApplicationId <- "AsQUVMODsS2NezEnDpl2DUlyGSoEa9EZTqL9lxcBTcKn2v53mfKJ-VOLufebS4Km"
              credentials
            )
      }

      HtmlPage.RegisterScriptableObject("Map", map)
      ()
    )