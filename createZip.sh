#!/bin/bash
rm -rf Styline.zip Styline.dll
dotnet build Code/Styline/Styline.csproj
zip Styline.zip -r LICENSE.txt everest.yaml Styline.dll Ahorn Content Graphics