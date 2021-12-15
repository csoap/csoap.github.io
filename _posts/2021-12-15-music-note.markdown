---
layout:     post
title:      "Unity下的音频优化笔记"
subtitle:   ""
date:       2021-12-15
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 音频
---
- 文件格式
    - mp3 失真小，适合音质要求高的文件， 较长的音频，例如BGM
    - wav 未压缩音频，资源大，但是声音在播放是不需要解码，不推荐
    - ogg 压缩比高，适合人声、音效等
- 导入期设置
    - ForceToMono:是否单声道 若启用，此音频将向下混合成单声道声音，一般手机使用单声道足够满足需求
    - SampleRateSetting:语音、音效选OverrideSampleRate的22050KHz就足够了，保留初始音质可选PreserveSampleRate，项目中根据压缩比选择，22050 44100
    - preloadAudioData=false 不要让声音加载阻塞场景
    - LoadInBackground:勾选的音频文件如果直接放在场景里，会在场景加载的时候一起被加载，一般false
- 参数解释
    - OriginalSize:磁盘大小
    - ImportedSize:内存大小
    - 时延敏感：是否需要很快响应，这个会影响压缩格式的选择
- LoadType 加载方式
    - DecompressOnLoad 加载后直接全部解压到内存中，所以内存占用最多，直接就是音频资源大小（未压缩），也因此后续再播放时cpu性能消耗最小。只用于很小的音效。
    - CompressedInMemory 加载文件到内存中(CPU消耗比较大)，不解压，用到时再解压，所以内存占用一般，就是音频文件大小（压缩后），外加解压缩用的缓存
    - Streaming 播放时，循环从磁盘读取一部分，解压，播放。内存占用只有用于缓存的200KB左右，cpu消耗最大，内存占用最小
- 压缩格式
    - 文件体积=原始文件体积x压缩率
    - Vorbis 压缩率大（3-40:1），解码慢
    - ADPCM 压缩率小（3.5:1），解码速度较快
    - PCM 未压缩，解码速度最快，体积最大
- 目标
    - 根据项目体量，权衡一个具体值。
        - 文件体积尽可能小。
            - ImportedSize多大，文件体积就是多大
        - 内存占用尽可能小
            - 合理设定音频导入设置，常用短音频可以考虑牺牲一部分内存来换取加载速度。
        - 效果能接受
- 实战
    - kow
        - sound 音效文件
            - forceMono = true，bSensitive = true， compressSampleRate = true
        - voice 角色声音
            - forceMono = true, bSensitive = false, compressSampleRate = true
        - music 背景音乐
            - forceMono = false, bSensitive = false, compressSampleRate = false
        - cg cg音乐
            - forceMono = false, bSensitive = false, compressSampleRate = true