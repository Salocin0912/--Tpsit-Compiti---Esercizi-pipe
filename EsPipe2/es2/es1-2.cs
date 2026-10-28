#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>    // Per open() e O_WRONLY
#include <unistd.h>   // Per write(), close()
#include <string.h>   // Per strlen()

#define FIFO_PATH "/tmp/fifo_echo"

int main() {
    char buffer[256];

    // Passo 1: Legge una riga da tastiera usando fgets
    printf("Inserisci un messaggio per il server: ");
    if (fgets(buffer, sizeof(buffer), stdin) == NULL) {
        perror("Errore lettura tastiera");
        exit(EXIT_FAILURE);
    }

    printf("[Client] Connessione al server...\n");

    // Passo 2: Apre la FIFO in scrittura (si sblocca quando il server è pronto)
    int fd = open(FIFO_PATH, O_WRONLY);
    if (fd == -1) {
        perror("Errore open client");
        exit(EXIT_FAILURE);
    }

    // Passo 3: Invia il messaggio al server
    write(fd, buffer, strlen(buffer) + 1);
    printf("[Client] Messaggio inviato!\n");

    // Passo 4: Chiude il canale
    close(fd);

    return 0;
}