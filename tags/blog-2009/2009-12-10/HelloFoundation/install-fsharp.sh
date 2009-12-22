#!/bin/sh
# script to download F# 1.9.7.8 and install its main assemblies in Mono's GAC
mkdir ~/mono
cd ~/mono
curl http://download.microsoft.com/download/E/B/3/EB34723E-23C9-4872-A1CD-6D12CF3B494A/fsharp.zip -o FSharp-1.9.7.8.zip
unzip FSharp-1.9.7.8.zip
cd FSharp-1.9.7.8
curl http://anonsvn.mono-project.com/source/trunk/mcs/class/mono.snk -O
sudo sh install-mono.sh
