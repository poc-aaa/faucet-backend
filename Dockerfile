FROM mcr.microsoft.com/dotnet/sdk:6.0 AS fetch-env
WORKDIR /App
COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/aelf-project-dev/api/v3/index.json

FROM fetch-env AS build-env
WORKDIR /App
ARG servicename
RUN dotnet publish src/$servicename/$servicename.csproj -o /output

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /output .
ARG servicename
ENV RUNCMD="dotnet $servicename.dll"
CMD $RUNCMD
