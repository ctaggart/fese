module GtkWindowExample

open Gtk

let main() =
  Application.Init()
  use wnd = new Window("Main Window")
  wnd.DeleteEvent.Add( fun args ->
    Application.Quit()
    args.RetVal <- true
  )
  wnd.ShowAll()
  Application.Run()
  ()

main()