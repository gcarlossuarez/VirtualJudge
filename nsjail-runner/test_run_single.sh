#!/usr/bin/bash
# NOTA. - Para facil acceso de nsjail, utilizar el directorio temporal tmp (de paso, se garantiza limpieza auotmatica).
# Es importante haber creado, desde el programa principal, el subdirectorio temporal con los datos del usuiario, obedeciendo
# a esta estructura:
# /tmp
#    -- /temp-workdir
#          -- /id<nro. aleatorio>
#                  -- /in
#                         -- Program.cs
#                         -- /IN
#                              datos0001.txt .....
#                         -- /OUT
#                              Output_datos0001.txt ....
#          -- /template
#                -- /App
#                       --  Solution.csproj
#sudo bash run_single.sh /tmp/tmp-workdir /tmp/tmp-workdir/id123/in 6 dotnet ""

sudo bash run_single.sh /tmp/temp-workdir /tmp/temp-workdir/id456/ 6 dotnet "/home/vboxuser/VirtualJudge/problems/8001/Validator8001/Validator.cs"

#sudo bash run_single.sh /tmp/temp-workdir /tmp/temp-workdir/id456cpp/ 6 g++ "/home/vboxuser/VirtualJudge/problems/8001/Validator8001/Validator.cs"

# Para ver los archivos de salida, a medida que se generan
#sudo bash run_single.sh /tmp/temp-workdir/id123/ 6 dotnet "" 1
