#r "System.Management.Automation"

open System
open System.Globalization
open System.Management.Automation
open System.Management.Automation.Runspaces
open System.Management.Automation.Host

type Host01 () =
    let mutable shouldExit = false
    let mutable exitCode = 0

    member x.ShouldExit
        with get () = shouldExit
        and set (value) = shouldExit <- value

    member x.ExitCode
        with get () = exitCode
        and set (value) = exitCode <- value

    member x.Eval script =
        let me = new Host01 ()
        let host = new MyHost (me)
        use myRunspace = RunspaceFactory.CreateRunspace (host)
        myRunspace.Open ()
        use powershell = PowerShell.Create ()
        powershell.Runspace <- myRunspace
        powershell.AddScript (script) |> ignore
        let rv = powershell.Invoke (script)
        printfn "%A" rv

and MyHost (prog : Host01) =
    inherit PSHost ()

    let program = prog
    let guid = Guid.NewGuid ()
    let mutable originalCultureInfo  =
        System.Threading.Thread.CurrentThread.CurrentCulture
    let mutable originalUICultureInfo  =
        System.Threading.Thread.CurrentThread.CurrentUICulture

    override x.Name
        with get () = "foobar"

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

let h = Host01()
h.Eval "[math]::Pow(2,10)"
