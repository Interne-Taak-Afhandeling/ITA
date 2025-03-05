{{- define "internetaakafhandeling.poller.fullname" -}}
{{- printf "%s-%s" .Release.Name "poller" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "internetaakafhandeling.poller.appsettings" -}}
{{ toYaml .Values.appsettings | nindent 2 }}
{{- end }}
