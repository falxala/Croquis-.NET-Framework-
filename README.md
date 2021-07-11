# Croquis
 
 ![説明](https://raw.githubusercontent.com/falxala/Croquis-.NET-Framework-/main/Croquis(.NET%20Framework)/Resources/manual1.jpg)

----
## このアプリはなーに？
お持ちの画像を順番に表示するアプリです（いわゆるスライドショー）  
クロッキーの効率化と支援のために独自の機能を備えています．

----
## このアプリでできること
- 15秒から15分の間で表示間隔の設定
- 画像切り替わり時の準備時間の設定
- 再生，一時停止，リピート
- 画像のグレースケール表示
- フルスクリーン表示
- グリッド表示
- シークバー（プログレスバー）表示
- 画像リスト表示
- 背景色の変更（白・黒）
- ランダム表示
- 透過画像表示

----
##  対応環境
Windows10 (バージョン1803以上)  
またはMicrosoft .NET Framework 4.7.2がインストールされているPC  
(メインメモリに1GB以上の空きが有ることを推奨します)

----
## 使い方
1. 「Croquis.exe」を起動後，画像ファイル(またはフォルダ)をドラッグアンドドロップするか，ダブルクリックで表示されるファイル追加ダイアログからファイルを追加します．  
2. ウィンドウを右クリックすることで表示されるメニューから表示時間等を設定します．  
3. スペースバーを押下するか，右クリックから”スライドショーを開始”をクリックすることでスライドショーが開始されます．

----
## 便利な機能

- 準備時間
  - クロッキーをしているとき，画像が切り替わるときにページをめくりたい場合があると思います．その際は右クリックメニューから準備時間をクリックすると切替時に待機時間が発生します．（デフォルトでは3秒です）

- リスト表示
  - Tabキーを押下することで画像リストが表示され,任意の画像を選択することができます．アイテムが選択された状態でDeleteキーを押下するとリストから除去することができます．

- ランダム表示
  - 画像リストの上にあるソート選択ボックスから”ランダム”を選択することで表示順がランダムになります．ボックスの上で右クリックをすると乱数を再生成することができ，再度，順番をバラバラにできます．

----
## ショートカットキー
以下に示すショートカットキーを用意しています  
- 【 ENTER 】
  - 全画面表示/解除
- 【 ESCAPE 】
  - 全画面表示の解除  
- 【 SPACE 】
  - スライドショーの一時停止・再開  
- 【 Tab 】
  - 画像リストを表示  
- 【 ← 】
  - 1枚戻る(画像リスト非表示のとき)  
- 【 → 】
  - 1枚進む(画像リスト非表示のとき) 
- 【 S or C 】
  - グレースケール（無彩色）表示  
- 【 B 】
  - バックグラウンドカラー変更（白・黒）  
- 【 G 】
  - グリッド表示  
- 【 ] 】
  - グリッドサイズ拡大
- 【 [ 】
  - グリッドサイズ縮小  
- 【 . 】
  - グリッド線を太く  
- 【 , 】
  - グリッド線を細く  
- 【 P 】
  - プログレスパーのオンオフ  
- 【 I 】
  - ステータス（残り時間，残り枚数表示）のオンオフ  
- 【 R 】
  - シークバーを0に戻す  
<br>

### 操作例
- 『表示時間3分，リピート表示したい時』
  - 右クリックメニューから"リピート"をクリック．同様に"表示間隔"にカーソルを合わせて目的の時間をクリック．

- 『画像をグレースケールにし，グリッドを表示したい時』
  - CまたはSキーを押し，その後Gキーを押します．グリッドのサイズを変更したい場合は"[]"(角括弧)を押します．

- 『次の画像を表示したい時』
  - 画像リストが表示されていない状態で右矢印キーを押します．前の画像は左矢印キーです．

----
## 設定のカスタマイズ 
- settings.configファイルを書き換えることでシークバーの色や大きさ，テキストのサイズなどが変更可能です．
例えば，settings.configファイルをメモ帳等で開き
  > \<ProgressBarColor>#FFFF3030\<ProgressBarColor>

  を  
  > \<ProgressBarColor>#0090a8\<ProgressBarColor>

  と変えるとプログレスバーの色が赤色から青緑色になります （先頭の二文字FFはアルファ値ですので無くても認識します）
  16進数コードはフォトショップなどで確認するかネットで検索してみてください
----
## 留意点
- 様々な環境を想定して32bitアプリケーションとしてビルドしています．そのため，余りにも大きな画像（8k,16k解像度等）は読み込めない可能性があります．その際は解像度を下げて再試行します．
----
## 免責事項
- このアプリを使用したことによって生じた損害について、当方は一切責任を負いません．
## 頒布について
- ユーザーはこのアプリのオリジナルまたはコピーを自由に再頒布することができます．

----
----
## ENGver
## What is this app?
It is an application that displays your images in order (so-called slideshow).  
It has its own features to streamline and support cropping.

----
## What you can do with this app
- Set the display interval between 15 seconds and 15 minutes.
- Set the preparation time when switching images.
- Play, pause, and repeat
- Grayscale display of images
- Full screen display
- Grid display
- Seek bar (progress bar) display
- Image list display
- Change background color (black/white)
- Random view
- Transparent image display

----
## Supported Environment
Windows 10 (version 1803 or higher)  
NET Framework 4.7.2 installed on your PC.  
(1GB or more of free space in the main memory is recommended)

----
## How to use
1. launch "Croquis.exe", drag and drop image files (or folders), or double-click to add files from the Add File dialog. 
2. set the display time and other settings from the menu displayed by right-clicking on the window. 
3. Press the space bar or right-click and click "Start Slide Show" to start the slide show.

----
## Useful Features

- Preparation time
  - When you are croqueting, you may want to turn the page when the image changes. In this case, you can click "Preparation Time" from the right-click menu. (The default value is 3 seconds.)

- List view
  - By pressing the Tab key, the image list is displayed. Any image can be selected. You can remove an item from the list by pressing the Delete key when the item is selected.

- Random Display
  - By selecting "Random" from the sort selection box above the image list, the display order will be randomized. By right-clicking on the box, you can regenerate the random numbers and change the order again.

----
## Shortcut keys
The following shortcut keys are available  
- ENTER
  - Full Screen Display / Release
- ESCAPE
  - Cancel Full Screen Display  
- SPACE
  - Pause/resume the slideshow
- Pause/resume slideshow
  - Display the image list
- ←
  - Go back one image (when the image list is not displayed)
- →
  - Go forward one image (when the image list is not displayed)
- S or C 
  - Grayscale (achromatic) display  
- B 
  - Change the background color (white or black)
- G
  - Grid Display  
- ]
  - Increase grid size
- [
  - Reduce grid size  
- .
  - Make the grid lines thicker  
- , 
  - Make the grid lines thinner  
- P
  - Turn on/off the progress bar
- I
  - Turn on/off Status (Remaining Time, Remaining Number of Images)
- R
  - Reset the seek bar to 0
<BR>

### Operation Example
- When you want to repeat the display time of 3 minutes.
  - Click "Repeat" from the right-click menu. Click "Repeat" from the right-click menu. Similarly, move the cursor to "Display Interval" and click the desired time.

- When you want to change the image to grayscale and display a grid.
  - Press the C or S key, and then press the G key. If you want to resize the grid, press "[]" (square brackets).

- When you want to display the next image
  - Press the right arrow key when the image list is not displayed. For the previous image, press the left arrow key.

----
## Customizing the settings 
- You can change the color, size, text size, etc. of the seek bar by rewriting the settings.config file.
For example, open the settings.config file with Notepad, etc.
  > \<ProgressBarColor>#FFFF3030\<ProgressBarColor>

  to  
  > \<ProgressBarColor>#0090a8\<ProgressBarColor>\.

  to change the progress bar color from red to blue-green (the first two letters FF are alpha values and will be recognized without them).
  (The first two letters FF are alpha values and will be recognized without them.
----
## Notes
- This application is built as a 32-bit application for various environments. Therefore, it may not be possible to load images that are too large (8k, 16k resolution, etc.). In that case, we will try again with a lower resolution.
----
## Disclaimer
- We are not responsible for any damage caused by using this application.
## Distribution
- Users are free to redistribute the original or a copy of this application.
