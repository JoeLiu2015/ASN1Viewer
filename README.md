# ASN1Viewer

[![Build Status](https://github.com/JoeLiu2015/ASN1Viewer/actions/workflows/build.yml/badge.svg)](https://github.com/JoeLiu2015/ASN1Viewer/actions/workflows/build.yml)



A lightweight and fast ASN.1 data viewer, built in C# for Windows. Parses ASN.1 data or files(*.pfx, .p7b, *.pem, *.cer, *.der, etc) and displays it in a tree view.

## Features
- 🧬 Parses DER-encoded, PEM-encoded, HEX-encoded ASN.1
- 🌳 Tree-view UI
- 🪟 Built with WinForms
- 📦 Simple executable, no install needed

## Screenshots
![Run](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Asn1Viewer.gif)

## Getting Started
1. Clone repo
2. Open in Visual Studio
3. Build and run
4. Open sample(Test) files
![TestFiles](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Test%20Files.png)
5. There are 3 ways to open ASN.1 data(file)
  - By menue (File->Open)
  - Drag the file into the Form
  - Input PEM text or HEX text in the **"Input Text"** tab

  ![TreeView](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Pfx.png)

## Use cases
- Inspect X.509 certificates
- Analyze PKCS structures
- Debug ASN.1-based protocols

## License
MIT

