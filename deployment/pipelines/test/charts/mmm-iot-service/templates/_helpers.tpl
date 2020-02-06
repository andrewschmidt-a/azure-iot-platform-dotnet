{{/* vim: set filetype=mustache: */}}
{{- define "mmm-iot-service.name" -}}
  {{- required "Error: missing required value .Values.nameOverride" .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "mmm-iot-service.fullname" -}}
  {{- include "mmm-iot-service.name" . -}}
{{- end -}}

{{- define "mmm-iot-service.chart" -}}
  {{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "mmm-iot-service.labels" -}}
app.kubernetes.io/name: {{ include "mmm-iot-service.name" . }}
helm.sh/chart: {{ include "mmm-iot-service.chart" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "mmm-iot-service.image.repository" -}}
  {{- include "mmm-iot-service.name" . | cat "azureiot3m/" -}}
{{- end -}}
