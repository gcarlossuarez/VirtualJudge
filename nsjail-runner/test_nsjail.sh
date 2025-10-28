#!/bin/bash
# test_nsjail_corrected.sh

echo "=== Probando nsjail con bash ==="
sudo nsjail \
  -M o \
  --time_limit 5 \
  --rlimit_cpu 5 \
  --rlimit_as 268435456 \
  --rlimit_nproc 16 \
  --user 65534 \
  --group 65534 \
  --bindmount_ro /bin \
  --bindmount_ro /lib \
  --bindmount_ro /lib64 \
  --bindmount_ro /usr \
  --bindmount_ro /etc \
  --chroot /tmp \
  -- /bin/bash -c 'echo "✅ NsJail con bash FUNCIONA"'

echo -e "\n=== Probando nsjail con .NET (más memoria) ==="
sudo nsjail \
  -M o \
  --time_limit 10 \
  --rlimit_cpu 5 \
  --rlimit_as 536870912 \
  --rlimit_nproc 16 \
  --user 65534 \
  --group 65534 \
  --bindmount_ro /bin \
  --bindmount_ro /lib \
  --bindmount_ro /lib64 \
  --bindmount_ro /usr \
  --bindmount_ro /etc \
  --chroot /tmp \
  -- /usr/bin/dotnet --info || echo "❌ .NET necesita más configuración"

echo -e "\n=== Probando compilación simple ==="
echo 'print("Hola Python")' | sudo nsjail \
  -M o \
  --time_limit 5 \
  --rlimit_cpu 3 \
  --rlimit_as 134217728 \
  --user 65534 \
  --group 65534 \
  --bindmount_ro /bin \
  --bindmount_ro /lib \
  --bindmount_ro /lib64 \
  --bindmount_ro /usr \
  --bindmount_ro /etc \
  --chroot /tmp \
  -- /usr/bin/python3 -
