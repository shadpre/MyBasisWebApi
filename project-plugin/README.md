# MyBasisWebApi Projekt

Her ligger kilden til det private projektplugin. De kanoniske projektkrav findes i `docs/project/`; de kopieres ikke manuelt til en ekstra vedligeholdt version.

Pakning udføres fra en ren, committed kildeversion, hvis fulde SHA først er kontrolleret på GitHub:

```bash
python3 tools/package_project_plugin.py --source-sha <VERIFICERET-FULD-SHA> --output <ABSOLUT-ZIP-STI>
```

Scriptet samler manifest, projektets skill og daterede dokumentkopier. Relative dokumentlinks omskrives til kilder på den præcise commit. SOURCE.json registrerer både kilde- og pakkefilernes hashes. Arkivet er en pakke til Plugin Creator; det udgiver ikke pluginet selv.

Efter udgivelse læses pluginets faktiske identitet, version, release og filer tilbage. Kvitteringen føjes til projektstatus. Pluginets snapshot forbliver knyttet til den dokumenterede commit; nye sessioner kontrollerer den aktuelle status i repositoryet.

Produktets fireleddede version er uafhængig af pluginmanifestets SemVer. Denne første pluginversion er 0.1.0 og repræsenterer projektgrundlaget.
