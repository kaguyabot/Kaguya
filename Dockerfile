FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
COPY Kaguya/Kaguya /Kaguya
COPY References /References
COPY nuget.config /Kaguya
WORKDIR /Kaguya
RUN dotnet restore Kaguya.csproj
RUN dotnet publish Kaguya.csproj -c Release -o out --no-restore

FROM  mcr.microsoft.com/dotnet/aspnet:5.0
COPY --from=build /Kaguya/out /KaguyaApp
WORKDIR /KaguyaApp
EXPOSE 80
CMD ["dotnet", "Kaguya.dll"]