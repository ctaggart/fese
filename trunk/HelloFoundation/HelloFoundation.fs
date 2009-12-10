
open Monobjc
open Monobjc.Cocoa

// Load any required framework
ObjectiveCRuntime.LoadFramework("Cocoa");
// Do the bridge initialization
ObjectiveCRuntime.Initialize();

let main() =
  printfn "Hello from F#"
  use pool = new NSAutoreleasePool();
  printfn "Username: %A" (FoundationFramework.NSFullUserName())
  printfn "Home Dir: %A"(FoundationFramework.NSHomeDirectory())
  use dict = new NSDictionary(new NSString "/System/Library/Frameworks/AppKit.framework/Resources/version.plist")
  printfn "AppKit Version: %A" (dict.[new NSString("CFBundleVersion")].CastTo<NSString>())

main()