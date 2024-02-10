# A2SService
Channel | Status
-|-
CI | [![CI](https://github.com/HMBSbige/A2SService/workflows/CI/badge.svg)](https://github.com/HMBSbige/A2SService/actions)
A2SService | [![NuGet.org](https://img.shields.io/nuget/v/A2SService.svg?logo=nuget)](https://www.nuget.org/packages/A2SService/)
FakeA2SServer | [![Docker](https://img.shields.io/badge/fakea2sserver-blue?label=Docker&logo=docker)](https://github.com/users/HMBSbige/packages/container/package/fakea2sserver)


## Docker Compose Example
```yml
version: "3"
services:
   fakea2sserver:
      image: ghcr.io/hmbsbige/fakea2sserver
      restart: unless-stopped
      container_name: fakea2sserver
      ports:
         - 27015:27015/udp
      environment:
         - A2SName=原神
         - A2SGame=Genshin Impact
         - A2SMaxPlayers=255
         - A2SVisibility=Private
         - A2SA2SVacStatus=1
```