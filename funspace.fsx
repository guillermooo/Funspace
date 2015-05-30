#r "System.Management.Automation"

open System
open System.Globalization
open System.Management.Automation
open System.Management.Automation.Runspaces
open System.Management.Automation.Host

type Daemon () =
    let mutable shouldExit = false
    let mutable exitCode = 0

    [<DefaultValue>] val mutable me : Daemon
    [<DefaultValue>] val mutable host : PSDaemonHost
    [<DefaultValue>] val mutable myRunspace : Runspace

    member private x.init =
        x.me <- new Daemon ()
        x.host <- new PSDaemonHost (x.me)
        x.myRunspace <- RunspaceFactory.CreateRunspace (x.host)
        x.myRunspace.Open ()

    member x.ShouldExit
        with get () = shouldExit
        and set (value) = shouldExit <- value

    member x.ExitCode
        with get () = exitCode
        and set (value) = exitCode <- value

    member private x.eval script =
        use powershell = PowerShell.Create ()
        powershell.Runspace <- x.myRunspace
        powershell.AddScript (script) |> ignore
        let rv = powershell.Invoke (script)
        printfn "OUT: %A" rv

    member x.RunForever =
        x.init
        while not shouldExit do
            x.eval <| Console.ReadLine ()

and PSDaemonHost (program : Daemon) =
    inherit PSHost ()

    let program = program
    let guid = Guid.NewGuid ()
    let mutable originalCultureInfo  =
        System.Threading.Thread.CurrentThread.CurrentCulture
    let mutable originalUICultureInfo  =
        System.Threading.Thread.CurrentThread.CurrentUICulture

    override x.Name
        with get () = "PowerShellDaemonHost"

    override x.Version
        with get () = Version (0, 0, 1, 0)

    override x.InstanceId
        with get () = guid

    override x.UI
        with get () = null

    override x.CurrentCulture
        with get () = originalCultureInfo

    override x.CurrentUICulture
        with get () = originalUICultureInfo

    override x.SetShouldExit(code) =
        program.ShouldExit <- true
        program.ExitCode <- code

    override x.EnterNestedPrompt () =
        raise (new NotImplementedException ())

    override x.ExitNestedPrompt () =
        raise (new NotImplementedException ())

    override x.NotifyBeginApplication () =
        ()

    override x.NotifyEndApplication () =
        ()

let psdaemon = Daemon()
psdaemon.RunForever
