module QtHellWorld

open com.trolltech.qt.gui

let main() =
  QApplication.initialize [||]
  let btn = QPushButton "Hello World!"
  btn.setWindowTitle "Hello World Title"
  btn.resize(320,240)
  btn.show()
  QApplication.exec() |> ignore
  ()

main()