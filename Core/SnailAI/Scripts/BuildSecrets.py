
import argparse
import json
from pathlib import Path
import sys
from enum import Enum


parser = argparse.ArgumentParser(description="Build script for SnailAI Secrets")

parser.add_argument ('-d', '--directory', type=str, required= True, help='Project Directory')
parser.add_argument ('-c', '--clean', type=bool, required=False, default=False, help='Clean Secrets')

args = parser.parse_args()

directory = Path(args.directory)
secretsJsonPath = directory / "falConfig.json"
secretsCsPath = directory / "Secrets.cs"

blankSecrets ={
    "FAL-API-Key" : "",
	"LowAPIEndpoint" : "",
    "MedAPIEndpoint" : "",
    "HighAPIEndpoint" : ""
}



class ExitCode(Enum):
    SUCCESS = 0
    JSON_NOT_FOUND= 1
    INVALID_JSON = 2
    PERMISSION_DENIED = 3
    MISSING_SECRETS = 4
    UNKNOWN_ERROR = 99
    
def read_json(file_path):
    try:
        with file_path.open('r', encoding='utf-8') as file:
            loadedJson = json.load(file)
            if not validate_secrets_json(loadedJson):
                print(f"Missing keys or values in secrets json." ,file=sys.stderr)
                sys.exit(ExitCode.MISSING_SECRETS.value)
            else:
                return loadedJson
    except PermissionError:
        print(f"Permission Error for '{file_path}'.", file=sys.stderr)
        sys.exit(ExitCode.PERMISSION_DENIED.value)
    except json.JSONDecodeError as e:
        print(f"Failed to parse json '{file_path}'.", file=sys.stderr)
        sys.exit(ExitCode.INVALID_JSON.value)
    except FileNotFoundError:
        print(f"Json not found at '{file_path}'.", file=sys.stderr)
        sys.exit(ExitCode.JSON_NOT_FOUND.value)
    except Exception as e:
        print(f"An unexpected error occured: '{e}'")
        sys.exit(ExitCode.UNKNOWN_ERROR.value)


def validate_secrets_json(secretsJson):
    req_keys =['FAL-API-Key', 'LowAPIEndpoint', 'MedAPIEndpoint', 'HighAPIEndpoint']

    for key in req_keys:
        if key not in secretsJson:
            return False
        elif secretsJson[key] == "":
            return False
    return True

def generate_secrets_cs(secrets, output_path):

    secrets_cs_template = f"""// This file is auto-generated. Do not modify manually.
using System.Runtime.CompilerServices;

namespace SnailAI
{{
    public static class Secrets
    {{
        public const string ApiKey = "{secrets['FAL-API-Key']}";
        public const string LowAPIEndpoint = "{secrets['LowAPIEndpoint']}";
        public const string MedAPIEndpoint = "{secrets['MedAPIEndpoint']}";
        public const string HighAPIEndpoint = "{secrets['HighAPIEndpoint']}";
    }}
}}
"""

    try:
        with output_path.open('w', encoding='utf-8') as cs_file:
            cs_file.write(secrets_cs_template)
        print(f"Successfully generated '{output_path}'.")
    except PermissionError:
        print(f"Permission denied while writing to '{output_path}'.", file=sys.stderr)
        sys.exit(ExitCode.PERMISSION_DENIED.value)
    except Exception as e:
        print(f"Failed to generate '{output_path}': {e}", file=sys.stderr)
        sys.exit(ExitCode.UNKNOWN_ERROR.value)


def main():
    if args.clean == True:
        generate_secrets_cs(blankSecrets,secretsCsPath)
        print("Secrets Cleaned.")
        sys.exit(ExitCode.SUCCESS.value)

    else:
        generate_secrets_cs(read_json(secretsJsonPath),secretsCsPath)
        sys.exit(ExitCode.SUCCESS.value)
    

    

if __name__ == "__main__":
    main()