namespace Funslae

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open Microsoft.Maps.MapControl
open Microsoft.Maps.MapControl.Core

type App() as app =
  inherit Application()
  do
    app.Startup.Add(fun args ->
      let grid = Grid()

      let map = Map()
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

      app.RootVisual <- grid
      ()
    )