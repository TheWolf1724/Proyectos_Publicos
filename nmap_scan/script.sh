#!/bin/bash

# Definición de colores para la salida en terminal
greenColour="\e[0;32m\033[1m"
endColour="\033[0m\e[0m"
redColour="\e[0;31m\033[1m"
blueColour="\e[0;34m\033[1m"
yellowColour="\e[0;33m\033[1m"
purpleColour="\e[0;35m\033[1m"
turquoiseColour="\e[0;36m\033[1m"
grayColour="\e[0;37m\033[1m"

# Función para comprobar si nmap está instalado, si no lo está intenta instalarlo
nmaptest() {
  # Verifica si nmap existe en /usr/bin/nmap
  test -f /usr/bin/nmap
  if [ "$(echo $?)" -ne 0 ]; then
    # Intenta instalar nmap con apt-get
    sudo apt-get install nmap -y > /dev/null 2>&1
    if [ "$(echo $?)" -ne 0 ]; then
      # Si falla, intenta con dnf
      sudo dnf install nmap -y > /dev/null 2>&1
    elif [ "$(echo $?)" -ne 0 ]; then
      # Si falla, intenta con pacman
      sudo pacman -S nmap -y > /dev/null 2>&1
    fi
  fi
}

# Función para pedir una IP y comprobar si está activa
ip_destination() {
  clear
  echo -ne "$greenColour\n[?]$grayColour Introduce la IP: " && read ip
  # Hace ping a la IP introducida
  ping -c 1 $ip | grep "ttl" > /dev/null 2>&1
  if [ "$(echo $?)" -ne 0 ]; then
    echo -e "$redColour[!]$grayColour No se encuentra activa la IP"
    ip_destination # Si la IP no responde, se solicita de nuevo.
  fi
}

# Comprueba si se está ejecutando como root
if [ $(id -u) -ne 0 ]; then
        echo -e "\n$redColour[!]$grayColour Debes ser root para ejecutar el script -> (sudo $0)"
exit 1
else
    nmaptest # Comprueba/instala nmap
    clear
    ip_destination # Solicita la IP que se va a escanear.
    while true; do
      # Menú de opciones.
      echo -e "\n1) Escaneo rapido pero ruidoso"
      echo "2) Escaneo Normal"
      echo "3) Escaneo silencioso (Puede tardar un poco mas de lo normal)"
      echo "4) Escaneo de serviciosos y versiones"
      echo "5) Escaneo Completo"
      echo "6) Escaneo de protocolos UDP" 
      echo "7) Salir"
      echo -ne "$greenColour\n[?]$grayColour Seleccione una opcion: " && read opcion
      case $opcion in
       1)
       # Escaneo rápido (Ruidoso)
       clear && echo "Escaneando..." && nmap -p- --open --min-rate 5000 -T5 -sS -Pn -n -v $ip | grep -E "^[0-9]+\/[a-z]+\s+open\s+[a-z]+"
       ;;
       2)
       # Escaneo normal
       clear && echo "Escaneando..." && nmap -p- --open $ip | grep -E "^[0-9]+\/[a-z]+\s+open\s+[a-z]+"
       ;;
       3)
       # Escaneo silencioso
       clear && echo "Escaneando..." && nmap -p- -T2 -sS -Pn -f $ip | grep -E "^[0-9]+\/[a-z]+\s+open\s+[a-z]+"
       ;;
       4)
       # Escaneo de servicios y versiones
       clear && echo "Escaneando..." && nmap -sV -sC $ip
       ;;
       5)
       # Escaneo completo
       clear && echo "Escaneando..." && nmap -p- -sS -sV -sC --min-rate 5000 -n -Pn $ip
       ;;
       6)
       # Escaneo de protocolos UDP
       clear && echo "Escaneando..." && nmap -sU --top-ports 200 --min-rate=5000 -n -Pn $ip
       ;;
       7)
       # Salir del menú
       break
       ;;
       *)
        # Opción no válida
        echo -e "\n$redColour[!]$grayColour Opcion no encontrada"
        ;;
      esac
    done
fi


# Contención de errores ante una interrupción del script de forma manual. (Ctrl+C)
finish() {
    echo -e "\n\e[1;31mAtaque interrumpido. Saliendo...\e[0m"
    exit 130
}

# Captura la señal SIGINT (Ctrl+C) y finaliza el script de forma controlada.
trap finish SIGINT
