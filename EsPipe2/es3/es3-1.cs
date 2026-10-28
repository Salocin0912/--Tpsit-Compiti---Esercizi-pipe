#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <unistd.h>
#include <string.h>

#define FIFO_12 "/tmp/fifo_1_2" // Per inviare
#define FIFO_21 "/tmp/fifo_21" // Per ricevere

int main() {
    // Crea entrambe le FIFO
    mkfifo(FIFO_12, 0666);
    mkfifo(FIFO_21, 0666);

    printf("[Chat 1] Connessione in corso (ordine: prima scrittura, poi lettura)...\n");

    // ORDINE ANTI-DEADLOCK: Prima apre in scrittura, poi in lettura
    int fd_write = open(FIFO_12, O_WRONLY);
    int fd_read = open(FIFO_21, O_RDONLY);

    char msg_out[256];
    char msg_in[256];

    while (1) {
        // Legge da tastiera il messaggio da inviare
        printf("Tu (Chat 1): ");
        if (fgets(msg_out, sizeof(msg_out), stdin) == NULL) break;

        write(fd_write, msg_out, strlen(msg_out) + 1);

        // Controlla se l'utente vuole uscire
        if (strncmp(msg_out, "EXIT", 4) == 0) {
            printf("[Chat 1] Uscita dalla chat.\n");
            break;
        }

        // Aspetta e legge la risposta dell'altro programma
        memset(msg_in, 0, sizeof(msg_in));
        read(fd_read, msg_in, sizeof(msg_in));
        
        if (strncmp(msg_in, "EXIT", 4) == 0) {
            printf("[Chat 1] L'altro utente e' uscito.\n");
            break;
        }

        printf("Chat 2 scrive: %s", msg_in);
    }

    close(fd_write);
    close(fd_read);
    unlink(FIFO_12);
    unlink(FIFO_21);

    return 0;
}