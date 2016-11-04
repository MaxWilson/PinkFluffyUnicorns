module Components

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Fable.Import.PIXI
open Fable.Import.React
open Fable.Import.React_Extensions

module R = Fable.Helpers.React
open R.Props

let mutable speed = 10
type UnicornBox(canvasContainer: HTMLElement) =
    let mutable animate_id = 0.
    let renderer = Globals.autoDetectRenderer(canvasContainer.clientWidth, canvasContainer.clientHeight, [RendererOptions.BackgroundColor (float 0x1099bb); Resolution 1.; Transparent true]) |> unbox<SystemRenderer>
    member this.Destroy() =
        canvasContainer.removeChild(renderer.view) |> ignore
        window.cancelAnimationFrame animate_id
    member this.Setup() =
        canvasContainer.appendChild(renderer.view) |> ignore
        let stage = Container()
        let mutable count = 0
        let countingText =
            Fable.Import.PIXI.Text("Xpos: 0",
                [
                TextStyle.Font "bold italic 24px Arial"
                TextStyle.Fill (U2.Case1 "#3e1707")
                TextStyle.Align "center"
                TextStyle.Stroke (U2.Case1 "#a4410e")
               ])
        countingText.position.x <- 310.
        countingText.position.y <- canvasContainer.clientHeight
        countingText.anchor.x <- 0.5
        countingText.anchor.y <- 1.
        stage.addChild(countingText) |> ignore
        let pics = System.Collections.Generic.List<_>()
        let imgs = List.map Texture.fromImage [
                    "BabyUnicorn.jpg"
                    "NomesDrawing.jpg"
                    "Vampire.jpg"
        ]
        let randomColor =
            let colors = [
                "blue"
                "yellow"
                "orange"
                "black"
                "violet"
                "pink"
                "green"
                "darkpink"
                "darkblue"
                ]
            fun() ->
                let i = (JS.Math.random() * 1000. |> int) % colors.Length
                colors.[i]
        let randomNeigh =
            let sounds = [
                "BabyUnicorn.m4a"
                "GhostBoom.m4a"
                "Horse-hooves-sound.mp3"
                "I'm a fluffy unicorn.m4a"
                "blahblahblahblahblah.m4a"
                "is there a pig.m4a"
                "little pesnits.m4a"
                "not like that.m4a"
                "pooping out butterflies.m4a"
                "wrongwrongwrpong.m4a"
              ]
            for s in sounds do
                Audio.Create(s).load() // pre-load all the sounds
            fun() ->
                let i = (JS.Math.random() * 1000. |> int) % sounds.Length
                Audio.Create(sounds.[i]).play()
        let rec addUnicorn(x:float, y:float) =
            let pic = Container()
            let i = (JS.Math.random() * 1000. |> int) % imgs.Length
            let img = imgs.[i]
            let p = Sprite(img)
            p.anchor.x <- 0.5
            p.anchor.y <- 0.5
            let scale = 0.1 + JS.Math.random() * 0.15
            pic.addChild(p) |> ignore
            pic.addChild(Text("neigh!",[TextStyle.Align "center"; TextStyle.Font "bold italic 300%"; TextStyle.Fill (U2.Case1 (randomColor()))], position=Point(0., 100.), anchor=Point(0.5, 0.5))) |> ignore
            pic.position.x <- x
            pic.position.y <- y
            pic.position.x <- JS.Math.random() * 400.
            pic.position.y <- JS.Math.random() * 400.
            pic.scale <- Point(scale, scale)
            pic?velocity <- scale
            let onclick() =
              pic?velocity <- (pic?velocity |> unbox<float>) * -1. + (JS.Math.random() * 0.20 - 0.10)
              p.scale.x <- p.scale.x * -1. // flip pic but not text
              p.scale.x <- p.scale.x * 2.0
              p.scale.y <- p.scale.y * 2.0
              if p.scale.y > 100.0 then
                p.scale.x <- 0.01
                p.scale.y <- 0.01
                addUnicorn(p.position.x, p.position.y)
                addUnicorn(p.position.x, p.position.y)
            pic.on_click (fun e -> onclick()) |> ignore
            pic.on_tap(fun e -> onclick()) |> ignore
            pic.interactive <- true
            stage.addChild(pic) |> ignore
            pics.Add(pic)
            count <- count + 1
            countingText.text <- sprintf "%d little unicorns, running down the field..." count
            randomNeigh()
        addUnicorn(JS.Math.random() * 400.,JS.Math.random() * 400.) // add one unicorn to start with

        let rec animate(dt:float) =
            animate_id <- window.requestAnimationFrame(FrameRequestCallback animate)
            for pic in pics do
                pic.position.x <- (pic.position.x + (float speed * (pic?velocity |> unbox))) // use scale as a speed constant too, so little horses go slower
                if (pic.position.x > (renderer.view.width + pic.width * 0.5)) && (pic?velocity |> unbox<float>) > 0. then // right wrap
                    pic.position.x <- pic.width * -0.5
                if (pic.position.x < abs pic.width * -0.5) && (pic?velocity |> unbox<float>) < 0. then // left wrap
                    pic.position.x <- (renderer.view.width + (abs pic.width * 0.5))
            renderer.render(stage)

        animate 0. // start a pixi animation loop

let mutable unicornBox = Unchecked.defaultof<UnicornBox>

// create a PIXI box with an animation loop
type DisplayBox() =
    inherit React.Component<unit, unit>()
    let mutable canvasContainer = null
    member this.render() =
        R.div [ClassName "game-canvas-container"; Ref (fun x -> canvasContainer <- (x :?> HTMLElement))][]
    member this.componentDidMount() =
        unicornBox <- UnicornBox(canvasContainer)
        unicornBox.Setup()
    member this.componentWillUnmount() =
        unicornBox.Destroy()

type App() =
    inherit Component<unit, unit>()
    member this.render() =
        R.div [] [
            R.button [OnClick (fun (x: MouseEvent) -> speed <- (speed * 2); if speed > 100 then speed <- 1)][R.str "Faster!"]
            R.button [OnClick (fun (x: MouseEvent) -> unicornBox.Destroy(); unicornBox.Setup())][R.str "Reset"]
            R.com<DisplayBox,_,_>()[]
            ]