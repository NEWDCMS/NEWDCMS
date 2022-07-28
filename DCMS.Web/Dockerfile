#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-nanoserver-1803 AS build
WORKDIR /src
COPY ["DCMS.Web/DCMS.Web.csproj", "DCMS.Web/"]
COPY ["DCMS.Services/DCMS.Services.csproj", "DCMS.Services/"]
COPY ["DCMS.Core/DCMS.Core.csproj", "DCMS.Core/"]
COPY ["DCMS.Data/DCMS.Data.csproj", "DCMS.Data/"]
COPY ["DCMS.ViewModel/DCMS.ViewModel.csproj", "DCMS.ViewModel/"]
COPY ["DCMS.Web.Framework/DCMS.Web.Framework.csproj", "DCMS.Web.Framework/"]
RUN dotnet restore "DCMS.Web/DCMS.Web.csproj"
COPY . .
WORKDIR "/src/DCMS.Web"
RUN dotnet build "DCMS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DCMS.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DCMS.Web.dll"]