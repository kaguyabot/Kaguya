FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY Kaguya/Kaguya /Kaguya
COPY References /References
COPY nuget.config /Kaguya
WORKDIR /Kaguya
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef
RUN dotnet restore Kaguya.csproj
CMD ["dotnet", "ef", "database", "update"]