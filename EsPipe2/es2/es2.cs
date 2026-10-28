#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>
#include <signal.h> // Per gestire SIGPIPE

#define FIFO_PATH "/tmp/fifo_loop"

int main() {
    // Gestisce/ignora il segnale SIGPIPE per evitare crash se il server si chiude
    signal(SIGPIPE, SIG_IGN);

    int fd = open(FIFO_PATH, O_WRONLY);
    if (fd == -1) {
        perror("Errore open client");
        exit(EXIT_FAILURE);
    }

    char buffer[256];
    printf("Scrivi messaggi (scrivi QUIT per uscire):\n");

    while (1) {
        printf("Tu: ");
        if (fgets(buffer, sizeof(buffer), stdin) == NULL) break;

        // Invia il messaggio al server
        ssize_t res = write(fd, buffer, strlen(buffer) + 1);
        if (res == -1) {
            perror("Errore write (server chiuso?)");
            break;
        }

        // Se l'utente ha scritto QUIT, usciamo dal ciclo del client
        if (strncmp(buffer, "QUIT", 4) == 0) {
            break;
        }
    }

    close(fd);
    return 0;
}