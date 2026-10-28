#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>    // Per open() e O_RDONLY
#include <sys/stat.h> // Per mkfifo()
#include <unistd.h>   // Per read(), close(), unlink()

#define FIFO_PATH "/tmp/fifo_echo"

int main() {
    // Passo 1: Crea la FIFO nel filesystem
    if (mkfifo(FIFO_PATH, 0666) == -1) {
        perror("Avviso mkfifo (forse esiste gia')");
    }

    printf("[Server] In attesa del client...\n");

    // Passo 2: Apre la FIFO in lettura (si blocca finché il client non si connette)
    int fd = open(FIFO_PATH, O_RDONLY);
    if (fd == -1) {
        perror("Errore open server");
        exit(EXIT_FAILURE);
    }

    // Passo 3: Legge il messaggio inviato dal client
    char buffer[256];
    ssize_t n = read(fd, buffer, sizeof(buffer));

    if (n > 0) {
        // Stampa il messaggio ricevuto preceduto da [SERVER] come richiesto
        printf("[SERVER] %s", buffer);
    }

    // Passo 4: Chiude il file descriptor
    close(fd);

    // Passo 5: Rimuove la FIFO dal filesystem al termine
    if (unlink(FIFO_PATH) == -1) {
        perror("Errore unlink");
    }

    printf("[Server] Terminato.\n");
    return 0;
}