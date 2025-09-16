{{/* Common names and labels */}}
{{- define "internetaakafhandeling.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "internetaakafhandeling.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/* ConfigMap name helper */}}
{{- define "internetaakafhandeling.configMapName" -}}
{{- if .Values.configMapName -}}
{{- .Values.configMapName | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-config" (include "internetaakafhandeling.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{/* Secret name helper */}}
{{- define "internetaakafhandeling.secretName" -}}
{{- if .Values.secretName -}}
{{- .Values.secretName | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-secret" (include "internetaakafhandeling.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{/* Web specific names */}}
{{- define "internetaakafhandeling.web.name" -}}
{{- printf "%s-web" (include "internetaakafhandeling.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Poller specific names */}}
{{- define "internetaakafhandeling.poller.name" -}}
{{- printf "%s-poller" (include "internetaakafhandeling.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Common labels */}}
{{- define "internetaakafhandeling.labels" -}}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end -}}

{{/* Component labels */}}
{{- define "internetaakafhandeling.web.labels" -}}
{{ include "internetaakafhandeling.labels" . }}
app.kubernetes.io/name: web
app.kubernetes.io/component: web
{{- end -}}

{{- define "internetaakafhandeling.poller.labels" -}}
{{ include "internetaakafhandeling.labels" . }}
app.kubernetes.io/name: poller
app.kubernetes.io/component: poller
{{- end -}}

{{/* Selector labels */}}
{{- define "internetaakafhandeling.web.selectorLabels" -}}
app.kubernetes.io/name: {{ include "internetaakafhandeling.web.name" . }}
app.kubernetes.io/instance: {{ include "internetaakafhandeling.fullname" . }}
{{- end -}}

{{- define "internetaakafhandeling.poller.selectorLabels" -}}
app.kubernetes.io/name: {{ include "internetaakafhandeling.poller.name" . }}
app.kubernetes.io/instance: {{ include "internetaakafhandeling.fullname" . }}
{{- end -}}

{{/* Database connection string helper */}}
{{- define "internetaakafhandeling.databaseConnectionString" -}}
{{- if .Values.postgresql.enabled -}}
  {{- printf "Host=%s-postgresql;Port=%s;Database=%s;Username=%s;Password=%s;" (include "internetaakafhandeling.fullname" .) .Values.database.port .Values.database.name .Values.database.username .Values.database.password -}}
{{- else -}}
  {{- printf "Host=%s;Port=%s;Database=%s;Username=%s;Password=%s;" .Values.database.host .Values.database.port .Values.database.name .Values.database.username .Values.database.password -}}
{{- end -}}
{{- end -}}