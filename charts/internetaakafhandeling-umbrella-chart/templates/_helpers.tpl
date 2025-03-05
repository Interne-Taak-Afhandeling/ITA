{{- define "internetaakafhandeling.fullname" -}}
{{- printf "%s" .Release.Name -}}
{{- end -}}

{{- define "internetaakafhandeling.web.name" -}}
{{- printf "%s-web" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}