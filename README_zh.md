# RhythmLane
[English](README.md)  
## 项目简介
下落式音游及对应的乐谱编辑器。使用Unity及C#开发！  
* 目前下落式音游部分中，Holding键的判定存在问题，待修复；  
* 本项目目前不计划更新至Unity 6或团结引擎。本项目可能会继续进行次要内容更新。
    
![Main menu](screenshots/menu.png)

## 内容
### 乐谱编辑器
这部分内容由[setchi](https://github.com/setchi/NoteEditor)的作品改编。感谢这个优质项目！  
目前该编辑器支持：    
* 加载.wav格式音乐文件
* 编辑2-5轨的音游谱面，支持普通和长按两种音符
* 设置乐谱的节奏及延迟
* 将谱面文件保存为.json格式      
![Editor](screenshots/editor.png)  

### 下落式音游
* 将.wav和.json文件放入同一路径下，并在MusicSelect场景选择   
* 提供了自动播放模式    
![Music select](screenshots/select.png)  
![Game play](screenshots/game.png)  

### 设置
可以调整并保存以下设置：
* 分辨率，音量及触键音效
* 键位
* 音乐播放延迟
* 音符下落速度
![Settings](screenshots/settings.png)

## 使用方法
* 在release中下载应用程序及一个测试用乐谱  
* 或者使用Unity打开项目文件，开发用版本为：2022.3.57 f1c2。 

