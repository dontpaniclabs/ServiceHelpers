
' RawTcpServiceCert1 / RawTcpClientCert1
certmgr -del -r localmachine -s TrustedPeople -c -n RawTcpServiceCert1
certmgr -del -r localmachine -s TrustedPeople -c -n RawTcpClientCert1
certmgr -del -r localmachine -s My -c -n RawTcpServiceCert1
certmgr -del -r localmachine -s My -c -n RawTcpClientCert1

makecert.exe -sr LocalMachine -ss MY  -pe -sky exchange -n "CN=RawTcpServiceCert1"  RawTcpServiceCert1.cer
makecert.exe -sr LocalMachine -ss MY  -pe -sky exchange -n "CN=RawTcpClientCert1" RawTcpClientCert1.cer

certmgr.exe -add -r localmachine -s My -c -n RawTcpServiceCert1 -r  localmachine -s TrustedPeople
certmgr.exe -add -r localmachine -s My -c -n RawTcpClientCert1 -r localmachine -s TrustedPeople

' RawTcpServiceCert_2 / RawTcpClientCert_2

certmgr -del -r localmachine -s TrustedPeople -c -n RawTcpServiceCert_2
certmgr -del -r localmachine -s TrustedPeople -c -n RawTcpClientCert_2
certmgr -del -r localmachine -s My -c -n RawTcpServiceCert_2
certmgr -del -r localmachine -s My -c -n RawTcpClientCert_2

makecert.exe -sr LocalMachine -ss MY  -pe -sky exchange -n "CN=RawTcpServiceCert_2"  RawTcpServiceCert_2.cer
makecert.exe -sr LocalMachine -ss MY  -pe -sky exchange -n "CN=RawTcpClientCert_2" RawTcpClientCert_2.cer

certmgr.exe -add -r localmachine -s My -c -n RawTcpServiceCert_2 -r  localmachine -s TrustedPeople
certmgr.exe -add -r localmachine -s My -c -n RawTcpClientCert_2 -r localmachine -s TrustedPeople


' B2BCurrentUserClient / B2BCurrentUserService

certmgr -del -r localmachine -s TrustedPeople -c -n B2BCurrentUserService
certmgr -del -r localmachine -s TrustedPeople -c -n B2BCurrentUserService
certmgr -del -r localmachine -s My -c -n B2BCurrentUserService
certmgr -del -r localmachine -s My -c -n B2BCurrentUserService

makecert.exe -sr LocalMachine -ss MY -pe -sky exchange -n "CN=B2BCurrentUserService"  B2BCurrentUserService.cer
makecert.exe -sr LocalMachine -ss MY -pe -sky exchange -n "CN=B2BCurrentUserClient" B2BCurrentUserClient.cer

erase B2BCurrentUserService.pfx
erase B2BCurrentUserClient.pfx

certutil -privatekey -exportpfx "B2BCurrentUserService" B2BCurrentUserService.pfx
certutil -privatekey -exportpfx "B2BCurrentUserClient" B2BCurrentUserClient.pfx

' B2BLocalMachineClient / B2BLocalMachineService

certmgr -del -r localmachine -s TrustedPeople -c -n B2BLocalMachineService
certmgr -del -r localmachine -s TrustedPeople -c -n B2BLocalMachineService
certmgr -del -r localmachine -s My -c -n B2BLocalMachineService
certmgr -del -r localmachine -s My -c -n B2BLocalMachineService

makecert.exe -sr LocalMachine -ss MY -pe -sky exchange -n "CN=B2BLocalMachineService"  B2BLocalMachineService.cer
makecert.exe -sr LocalMachine -ss MY -pe -sky exchange -n "CN=B2BLocalMachineClient" B2BLocalMachineClient.cer

erase B2BLocalMachineService.pfx
erase B2BLocalMachineClient.pfx

certutil -privatekey -exportpfx "B2BLocalMachineService" B2BLocalMachineService.pfx
certutil -privatekey -exportpfx "B2BLocalMachineClient" B2BLocalMachineClient.pfx





