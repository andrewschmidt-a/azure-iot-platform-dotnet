// Copyright (c) Microsoft. All rights reserved.

import React from 'react';
import { PcsGrid } from 'components/shared';
import { translateColumnDefs } from 'utilities';
import { TimeRenderer } from 'components/shared/cellRenderers';

const columnDefs = [
  {
    headerName: 'tenantManagement.tenantGrid.tenantName',
    field: 'name'
  },
  {
    headerName: 'tenantManagement.tenantGrid.tenantStatus',
    field: 'status'
  }
];

export const TenantGrid = ({ t, ...props }) => {
  const gridProps = {
    columnDefs: translateColumnDefs(t, columnDefs),
    context: { t },
    sizeColumnsToFit: true,
    ...props
  };
  return (
    <PcsGrid {...gridProps} />
  );
};
