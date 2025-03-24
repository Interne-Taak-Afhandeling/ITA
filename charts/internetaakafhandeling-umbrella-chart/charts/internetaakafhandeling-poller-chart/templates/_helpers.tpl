{{- define "internetaakafhandeling.fullname" -}}
{{- printf "%s" .Release.Name -}}
{{- end -}}

{{- define "internetaakafhandeling.poller.fullname" -}}
{{- printf "%s-%s" .Release.Name "poller" | trunc 63 | trimSuffix "-" -}}
{{- end -}}