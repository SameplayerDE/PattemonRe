package main

import (
	"encoding/csv"
	"encoding/json"
	"fmt"
	"log"
	"os"
)

type Header struct {
	HeaderId       string `json:"headerId"`
	LocationName   string `json:"locationName"`
	ShowNameTag    bool   `json:"showNameTag"`
	AreaIcon       string `json:"areaIcon"`
	InternalName   string `json:"internalName"`
	MusicDayId     int    `json:"musicDayId"`
	MusicDayName   string `json:"musicDayName"`
	MusicNightId   int    `json:"musicNightId"`
	MusicNightName string `json:"musicNightName"`
	WeatherId      int    `json:"weatherId"`
	WeatherName    string `json:"weatherName"`
	CanUseFly      bool   `json:"canUseFly"`
	CanUseRope     bool   `json:"canUseRope"`
	CanUseRun      bool   `json:"canUseRun"`
	CanUseBicycle  bool   `json:"canUseBicycle"`
}

func main() {
	// Read the CSV file
	records := readCSV("MapHeaderMatrix.csv")

	var headers []Header
	headerSet := make(map[string]bool)

	// Parse the CSV data to extract unique headerId values
	for _, record := range records {
		for _, headerId := range record {
			if headerId != "" && !headerSet[headerId] {
				headerSet[headerId] = true
				header := Header{
					HeaderId:       headerId,
					LocationName:   "",
					ShowNameTag:    false,
					AreaIcon:       "",
					InternalName:   "",
					MusicDayId:     0,
					MusicDayName:   "",
					MusicNightId:   0,
					MusicNightName: "",
					WeatherId:      0,
					WeatherName:    "",
					CanUseFly:      false,
					CanUseRope:     false,
					CanUseRun:      false,
					CanUseBicycle:  false,
				}
				headers = append(headers, header)
			}
		}
	}

	// Convert to JSON
	jsonData, err := json.MarshalIndent(headers, "", "  ")
	if err != nil {
		log.Fatal("Error converting to JSON", err)
	}

	// Print the JSON
	fmt.Println(string(jsonData))

	// Save the JSON to a file
	if err := os.WriteFile("output.json", jsonData, 0644); err != nil {
		log.Fatal("Error writing JSON to file", err)
	}
}

func readCSV(filePath string) [][]string {
	file, err := os.Open(filePath)
	if err != nil {
		log.Fatal("Unable to read input file "+filePath, err)
	}
	defer file.Close()

	csvReader := csv.NewReader(file)
	records, err := csvReader.ReadAll()
	if err != nil {
		log.Fatal("Unable to parse file as CSV for "+filePath, err)
	}

	return records
}
