import React, { useState, useEffect } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { StressMap } from './components/StressMap';

import './custom.css'

export default function App() {
    return (
        <Layout>
            <Route exact path='/' component={Home} />
            <Route path='/map' component={StressMap} />
        </Layout>
    );
}
