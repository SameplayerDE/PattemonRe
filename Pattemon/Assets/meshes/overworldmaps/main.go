package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"path/filepath"
)

func main() {
	// Pfad zum Hauptordner, dessen Unterordner durchgegangen werden sollen
	rootFolder := "A:\\ModelExporter\\Platin\\overworldmaps"

	// Alle Unterordner im Hauptordner auflisten
	subfolders, err := ioutil.ReadDir(rootFolder)
	if err != nil {
		log.Fatalf("Fehler beim Lesen des Hauptordners: %v", err)
	}

	// Durchgehen der Unterordner
	for _, subfolder := range subfolders {
		if !subfolder.IsDir() {
			continue // Skip non-directory entries
		}

		subfolderPath := filepath.Join(rootFolder, subfolder.Name())

		// Alle Dateien im aktuellen Unterordner auflisten
		files, err := ioutil.ReadDir(subfolderPath)
		if err != nil {
			log.Printf("Fehler beim Lesen von Unterordner %s: %v", subfolder.Name(), err)
			continue
		}

		// Durchgehen der Dateien im Unterordner
		for _, file := range files {
			if file.IsDir() || (filepath.Ext(file.Name()) != ".glb" && filepath.Ext(file.Name()) != ".gltf") {
			    continue // Skip directories and non-.glb or non-.gltf files
			}

			oldFilePath := filepath.Join(subfolderPath, file.Name())

			// Neuer Dateiname basierend auf dem Unterordner-Namen
			newFileName := subfolder.Name() + filepath.Ext(file.Name())
			newFilePath := filepath.Join(subfolderPath, newFileName)

			// Umbenennen der Datei
			err := os.Rename(oldFilePath, newFilePath)
			if err != nil {
				log.Printf("Fehler beim Umbenennen von Datei %s: %v", oldFilePath, err)
				continue
			}

			fmt.Printf("Datei %s erfolgreich in %s umbenannt.\n", oldFilePath, newFilePath)
		}
	}
}
