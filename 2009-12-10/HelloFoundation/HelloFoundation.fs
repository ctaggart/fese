
open Monobjc
open Monobjc.Cocoa

let main() =
  printfn "Hello from F#"

  ObjectiveCRuntime.LoadFramework("Cocoa");
  ObjectiveCRuntime.Initialize();
  use pool = new NSAutoreleasePool();

  printfn "Username: %A" (FoundationFramework.NSFullUserName())
  printfn "Home Dir: %A"(FoundationFramework.NSHomeDirectory())
  use dict = new NSDictionary(new NSString "/System/Library/Frameworks/AppKit.framework/Resources/version.plist")
  printfn "AppKit Version: %A" (dict.[new NSString("CFBundleVersion")].CastTo<NSString>())
  ()

main()